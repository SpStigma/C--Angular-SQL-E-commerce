using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Dtos;
using server.Models;
using System.Security.Claims;

namespace server.Controllers
{
    /// <summary>
    /// Manages shopping cart operations for the authenticated user.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        /// <summary>
        /// Initializes a new instance of <see cref="CartController"/>.
        /// </summary>
        /// <param name="context">The EF Core database context.</param>
        public CartController(AppDbContext context)
        {
            _context = context;
        }

        /// <summary>
        /// Retrieves the current user's cart, items, total cost and item count.
        /// </summary>
        /// <returns>An <see cref="OkObjectResult"/> containing cart details, or <see cref="UnauthorizedResult"/>.</returns>
        [HttpGet]
        public async Task<IActionResult> GetCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
            {
                return Ok(new { items = new List<object>(), total = 0m, itemCount = 0 });
            }

            var items = cart.Items.Select(i => new
            {
                productId = i.ProductId,
                name      = i.Product?.Name,
                price     = i.Product?.Price,
                quantity  = i.Quantity
            }).ToList();

            var total     = items.Sum(i => i.price * i.quantity);
            var itemCount = items.Sum(i => i.quantity);

            return Ok(new { items, total, itemCount });
        }

        /// <summary>
        /// Adds a specified quantity of a product to the user's cart.
        /// </summary>
        /// <param name="dto">Contains <see cref="UpdateCartItemDto.ProductId"/> and <see cref="UpdateCartItemDto.Quantity"/>.</param>
        /// <returns><see cref="OkObjectResult"/> on success, <see cref="NotFoundObjectResult"/> if product missing, or <see cref="BadRequestObjectResult"/> if stock insufficient.</returns>
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });
            if (product.Stock < dto.Quantity)
                return BadRequest(new { message = $"Stock insuffisant pour '{product.Name}'" });

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId)
                ?? new Cart { UserId = userId.Value };

            if (cart.Id == 0)
            {
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item != null)
            {
                if (product.Stock < item.Quantity + dto.Quantity)
                    return BadRequest(new { message = $"Pas assez de stock pour ajouter {dto.Quantity} unités à '{product.Name}'" });
                item.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem { ProductId = dto.ProductId, Quantity = dto.Quantity });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit ajouté au panier" });
        }

        /// <summary>
        /// Removes a product from the user's cart.
        /// </summary>
        /// <param name="productId">The ID of the product to remove.</param>
        /// <returns><see cref="OkObjectResult"/> on success or <see cref="NotFoundObjectResult"/> if missing.</returns>
        [HttpDelete("remove/{productId}")]
        public async Task<IActionResult> RemoveItem(int productId)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NotFound(new { message = "Panier introuvable" });

            var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
            if (item == null) return NotFound(new { message = "Produit non dans le panier" });

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit retiré du panier" });
        }

        /// <summary>
        /// Updates the quantity of a given product in the cart, or removes it if quantity ≤ 0.
        /// </summary>
        /// <param name="dto">Contains <see cref="UpdateCartItemDto.ProductId"/> and the new <see cref="UpdateCartItemDto.Quantity"/>.</param>
        /// <returns><see cref="OkObjectResult"/> on success or <see cref="NotFoundObjectResult"/> if cart or item not found.</returns>
        [HttpPut("update")]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null) return NotFound(new { message = "Panier introuvable" });

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null) return NotFound(new { message = "Produit non dans le panier" });

            if (dto.Quantity <= 0)
                cart.Items.Remove(item);
            else
                item.Quantity = dto.Quantity;

            await _context.SaveChangesAsync();
            return Ok(new { message = dto.Quantity <= 0 ? "Produit retiré" : "Quantité mise à jour" });
        }

        /// <summary>
        /// Clears all items from the user's cart.
        /// </summary>
        /// <returns><see cref="OkObjectResult"/> indicating result.</returns>
        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any())
                return Ok(new { message = "Panier déjà vide" });

            cart.Items.Clear();
            await _context.SaveChangesAsync();
            return Ok(new { message = "Panier vidé" });
        }

        /// <summary>
        /// Returns the total price and item count of the user's cart.
        /// </summary>
        /// <returns>An object with <c>total</c> and <c>itemCount</c>.</returns>
        [HttpGet("total")]
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items).ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);
            if (cart == null || !cart.Items.Any())
                return Ok(new { total = 0, itemCount = 0 });

            var total     = cart.Items.Sum(i => i.Product != null ? i.Product.Price * i.Quantity : 0);
            var itemCount = cart.Items.Sum(i => i.Quantity);

            return Ok(new { total, itemCount });
        }

        /// <summary>
        /// Extracts the authenticated user's ID from JWT claims.
        /// </summary>
        /// <returns>User ID or null if not authenticated.</returns>
        private int? GetUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));
            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }
    }
}
