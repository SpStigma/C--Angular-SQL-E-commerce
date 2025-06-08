using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System.Security.Claims;

namespace server.Controllers
{
    /// <summary>
    /// Handles order placement and retrieval for authenticated users.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of the <see cref="OrdersController"/> class.
        /// </summary>
        /// <param name="context">The database context.</param>
        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Places an order for the current user's cart contents.
        /// </summary>
        /// <returns>
        /// <see cref="OkObjectResult"/> with the created <see cref="Order"/> on success; 
        /// <see cref="UnauthorizedResult"/> if not authenticated; 
        /// <see cref="BadRequestObjectResult"/> if cart is empty or stock insufficient.
        /// </returns>
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = GetUserId();
            if (userId == null) 
                return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return BadRequest(new { message = "Le panier est vide" });

            // Vérification de stock
            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                    return BadRequest(new { message = $"Produit {item.ProductId} introuvable" });
                if (item.Product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Stock insuffisant pour le produit '{item.Product.Name}'" });
            }

            // Préparation des OrderItem
            var orderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity  = item.Quantity,
                UnitPrice = item.Product!.Price
            }).ToList();

            var total = orderItems.Sum(i => i.Quantity * i.UnitPrice);

            var order = new Order
            {
                UserId      = userId.Value,
                Items       = orderItems,
                TotalAmount = total,
                Status      = OrderStatus.Pending
            };

            _context.Orders.Add(order);

            // Mise à jour du stock et vidage du panier
            foreach (var item in cart.Items)
                item.Product!.Stock -= item.Quantity;
            cart.Items.Clear();

            await _context.SaveChangesAsync();
            return Ok(order);
        }

        /// <summary>
        /// Retrieves all orders placed by the current user.
        /// </summary>
        /// <returns>
        /// <see cref="OkObjectResult"/> with a list of <see cref="Order"/>; 
        /// <see cref="UnauthorizedResult"/> if not authenticated.
        /// </returns>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null) 
                return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId.Value)
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Retrieves all orders (admin only).
        /// </summary>
        /// <returns>
        /// <see cref="OkObjectResult"/> with a list of <see cref="Order"/>.
        /// </returns>
        [HttpGet]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }

        /// <summary>
        /// Extracts the authenticated user's ID from JWT claims.
        /// </summary>
        /// <returns>The user ID, or <c>null</c> if not found or invalid.</returns>
        private int? GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
