using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using Stripe;
using Stripe.Checkout;
using System.Text.Json;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _context;

        public StripeWebhookController(IConfiguration config, AppDbContext context)
        {
            _config = config;
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            var secret = _config["Stripe:WebhookSecret"];
            Event stripeEvent;

            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    secret
                );
            }
            catch (StripeException e)
            {
                return BadRequest(new { message = "Signature Stripe invalide", error = e.Message });
            }

            if (stripeEvent.Type == "payment_intent.succeeded")
            {
                var paymentIntent = stripeEvent.Data.Object as PaymentIntent;

                if (paymentIntent != null && paymentIntent.Metadata.TryGetValue("order_id", out string? orderIdStr)
                    && int.TryParse(orderIdStr, out int orderId))
                {
                    var order = await _context.Orders.FirstOrDefaultAsync(o => o.Id == orderId);
                    if (order != null)
                    {
                        order.Status = OrderStatus.Paid;
                        await _context.SaveChangesAsync();
                    }
                }
            }

            return Ok();
        }
    }
}
