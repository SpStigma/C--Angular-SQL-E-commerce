using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Dtos;
using Stripe;
using System.Security.Claims;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PaymentsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PaymentsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpPost("create-intent")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] CreatePaymentIntentDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == dto.OrderId && o.UserId == userId);

            if (order == null)
                return NotFound(new { message = "Commande introuvable" });

            if (!string.IsNullOrEmpty(order.StripePaymentIntentId))
                return BadRequest(new { message = "Paiement déjà initié pour cette commande." });

            var options = new PaymentIntentCreateOptions
            {
                Amount = (long)(order.TotalAmount * 100), // Stripe utilise les centimes
                Currency = "eur",
                Metadata = new Dictionary<string, string>
                {
                    { "order_id", order.Id.ToString() },
                    { "user_id", userId.Value.ToString() }
                }
            };

            var service = new PaymentIntentService();
            var paymentIntent = await service.CreateAsync(options);

            order.StripePaymentIntentId = paymentIntent.Id;
            await _context.SaveChangesAsync();

            return Ok(new
            {
                clientSecret = paymentIntent.ClientSecret,
                paymentIntentId = paymentIntent.Id
            });
        }

        private int? GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
