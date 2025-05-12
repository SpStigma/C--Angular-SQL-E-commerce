using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Data;
using server.Dtos;
using server.Models;
using System.Security.Claims;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CartController(AppDbContext context)
        {
            _context = context;
        }

    [HttpGet]
    public async Task<IActionResult> GetCart()
    {
        var userId = GetUserId();
        if (userId == null) return Unauthorized();

        var cart = await _context.Carts
            .Include(c => c.Items)
            .ThenInclude(i => i.Product)
            .FirstOrDefaultAsync(c => c.UserId == userId);

        if (cart == null || !cart.Items.Any())
        {
            return Ok(new
            {
                items     = new List<object>(),
                total     = 0m,
                itemCount = 0
            });
        }

        var items = cart.Items.Select(i => new
        {
            productId = i.ProductId,
            name      = i.Product?.Name,
            price     = i.Product?.Price,
            quantity  = i.Quantity
        }).ToList();

        var total = items.Sum(i => i.price * i.quantity);
        var itemCount = items.Sum(i => i.quantity);

        return Ok(new
        {
            items,
            total,
            itemCount
        });
    }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var product = await _context.Products.FindAsync(dto.ProductId);
            if (product == null)
            {
                return NotFound(new { message = "Produit introuvable" });
            }

            if (product.Stock < dto.Quantity)
            {
                return BadRequest(new { message = $"Stock insuffisant pour '{product.Name}'" });
            }

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
            {
                cart = new Cart { UserId = userId.Value };
                _context.Carts.Add(cart);
                await _context.SaveChangesAsync();
            }

            var cartItem = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (cartItem != null)
            {
                if (product.Stock < cartItem.Quantity + dto.Quantity)
                {
                    return BadRequest(new { message = $"Pas assez de stock pour ajouter {dto.Quantity} unités à '{product.Name}'" });
                }

                cartItem.Quantity += dto.Quantity;
            }
            else
            {
                cart.Items.Add(new CartItem
                {
                    ProductId = dto.ProductId,
                    Quantity = dto.Quantity
                });
            }

            await _context.SaveChangesAsync();
            return Ok(new { message = "Produit ajouté au panier" });
        }

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
            if (item == null) return NotFound(new { message = "Produit introuvable dans le panier" });

            cart.Items.Remove(item);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Produit retiré du panier" });
        }

        private int? GetUserId()
        {
            var claim = User.Claims
                .FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier && int.TryParse(c.Value, out _));

            return claim != null && int.TryParse(claim.Value, out int id) ? id : null;
        }


        [HttpPut("update")]
        public async Task<IActionResult> UpdateItemQuantity([FromBody] UpdateCartItemDto dto)
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null)
                return NotFound(new { message = "Panier introuvable" });

            var item = cart.Items.FirstOrDefault(i => i.ProductId == dto.ProductId);
            if (item == null)
                return NotFound(new { message = "Produit non présent dans le panier" });

            if (dto.Quantity <= 0)
            {
                cart.Items.Remove(item);
                await _context.SaveChangesAsync();
                return Ok(new { message = "Produit retiré du panier" });
            }

            item.Quantity = dto.Quantity;
            await _context.SaveChangesAsync();
            return Ok(new { message = "Quantité mise à jour" });
        }


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

            return Ok(new { message = "Panier vidé avec succès" });
        }

        [HttpGet("total")]
        public async Task<IActionResult> GetCartTotal()
        {
            var userId = GetUserId();
            if (userId == null) return Unauthorized();

            var cart = await _context.Carts
                .Include(c => c.Items)
                .ThenInclude(i => i.Product)
                .FirstOrDefaultAsync(c => c.UserId == userId);

            if (cart == null || !cart.Items.Any())
                return Ok(new { total = 0, itemCount = 0 });

            var total = cart.Items.Sum(i => i.Product != null ? i.Product.Price * i.Quantity : 0);
            var itemCount = cart.Items.Sum(i => i.Quantity);

            return Ok(new
            {
                total,
                itemCount
            });
        }
    }
}
