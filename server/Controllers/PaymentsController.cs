using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using server.Data;
using server.Dtos;
using server.Models;
using Stripe;
using Stripe.Checkout;
using System.Security.Claims;

namespace server.Controllers
{
    /// <summary>
    /// Manages Stripe payment sessions for orders.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="PaymentsController"/> class.
        /// </summary>
        /// <param name="context">The application database context.</param>
        /// <param name="stripeOptions">Stripe settings (contains the secret key).</param>
        public PaymentsController(
            AppDbContext context,
            IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            // Configure Stripe secret key for each request
            StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
        }

        /// <summary>
        /// Creates a Stripe Checkout session for the specified order.
        /// </summary>
        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto dto)
        {
            var userId = GetUserId();
            if (userId == null) 
                return Unauthorized();

            var order = await _context.Orders
                .Include(o => o.Items).ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Commande introuvable" });
            if (!string.IsNullOrEmpty(order.StripeSessionId))
                return BadRequest(new { message = "Session déjà créée" });

            var lineItems = order.Items.Select(oi => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount  = (long)(oi.Product!.Price * 100),
                    Currency    = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = oi.Product.Name
                    }
                },
                Quantity = oi.Quantity
            }).ToList();

            // Determine the origin for success/cancel URLs (e.g., the SPA origin)
            var originHeader = Request.Headers["Origin"].ToString();
            var origin = !string.IsNullOrEmpty(originHeader)
                ? originHeader.TrimEnd('/')
                : $"{Request.Scheme}://{Request.Host}";

            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems          = lineItems,
                Mode               = "payment",
                SuccessUrl         = $"{origin}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl          = $"{origin}/cart",
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "user_id",  userId.Value.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions);

            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return Ok(new { sessionId = session.Id, checkoutUrl = session.Url });
        }

        /// <summary>
        /// Extracts the authenticated user's ID from JWT claims.
        /// </summary>
        /// <returns>The user ID, or <c>null</c> if not authenticated or invalid.</returns>
        private int? GetUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
