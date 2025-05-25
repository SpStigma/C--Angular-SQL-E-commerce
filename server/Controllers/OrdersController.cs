using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Models;
using System.Security.Claims;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OrdersController(AppDbContext context)
        {
            _context = context;
        }

        // POST /api/orders
        [HttpPost]
        public async Task<IActionResult> PlaceOrder()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return BadRequest(new { message = "Le panier est vide" });
            }

            // Vérifie que tous les produits ont du stock suffisant
            foreach (var item in cart.Items)
            {
                if (item.Product == null)
                    return BadRequest(new { message = $"Produit {item.ProductId} introuvable" });

                if (item.Product.Stock < item.Quantity)
                    return BadRequest(new { message = $"Stock insuffisant pour le produit '{item.Product.Name}'" });
            }

            // Prépare les items de la commande
            var orderItems = cart.Items.Select(item => new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Product!.Price
            }).ToList();

            var total = orderItems.Sum(i => i.Quantity * i.UnitPrice);

            var order = new Order
            {
                UserId = userId.Value,
                Items = orderItems,
                TotalAmount = total,
                Status = OrderStatus.Pending
            };

            _context.Orders.Add(order);

            foreach (var item in cart.Items)
            {
                item.Product!.Stock -= item.Quantity;
            }

            // Vide le panier
            cart.Items.Clear();

            await _context.SaveChangesAsync();

            return Ok(order);
        }

        // 1) GET api/orders/my → commandes de l'utilisateur
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Order>>> GetMyOrders()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var orders = await _context.Orders
                .Where(o => o.UserId == userId.Value)
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }

        // 2) GET api/orders → toutes les commandes (admin uniquement)
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Order>>> GetAllOrders()
        {
            var orders = await _context.Orders
                .Include(o => o.Items)
                .ToListAsync();

            return Ok(orders);
        }

        // 🔐 Utilitaire pour récupérer l'ID de l'utilisateur connecté
        private int? GetUserId()
        {
            var claim = User.Claims.FirstOrDefault(c =>
                c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
