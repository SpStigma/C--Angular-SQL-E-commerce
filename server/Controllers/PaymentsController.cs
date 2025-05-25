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
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(
            AppDbContext context,
            IOptions<StripeSettings> stripeOptions)
        {
            _context = context;
            // Configurez votre clé secrète Stripe à chaque requête
            StripeConfiguration.ApiKey = stripeOptions.Value.SecretKey;
        }

        [HttpPost("create-checkout-session")]
        public async Task<IActionResult> CreateCheckoutSession([FromBody] CreateCheckoutSessionDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            // On inclut Items puis Product pour construire les lignes Stripe
            var order = await _context.Orders
                .Include(o => o.Items)
                    .ThenInclude(oi => oi.Product)
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Commande introuvable" });

            if (!string.IsNullOrEmpty(order.StripeSessionId))
                return BadRequest(new { message = "Session Stripe déjà créée pour cette commande." });

            var lineItems = order.Items.Select(oi => new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(oi.Product!.Price * 100),
                    Currency = "eur",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = oi.Product.Name
                    }
                },
                Quantity = oi.Quantity
            }).ToList();

            var domain = $"{Request.Scheme}://{Request.Host}";
            var sessionOptions = new SessionCreateOptions
            {
                PaymentMethodTypes = new List<string> { "card" },
                LineItems = lineItems,
                Mode = "payment",
                SuccessUrl = $"{domain}/payment-success?session_id={{CHECKOUT_SESSION_ID}}",
                CancelUrl = $"{domain}/cart",
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "user_id", userId.Value.ToString() }
                }
            };

            var service = new SessionService();
            var session = await service.CreateAsync(sessionOptions);

            // On sauvegarde l'ID de session Stripe
            order.StripeSessionId = session.Id;
            await _context.SaveChangesAsync();

            return Ok(new { sessionId = session.Id, checkoutUrl = session.Url });
        }

        private int? GetUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
