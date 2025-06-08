using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using server.Data;
using server.Dtos;
using server.Models;

namespace server.Controllers
{
    /// <summary>
    /// Provides CRUD operations for products and image uploads.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _env;

        /// <summary>
        /// Initializes a new instance of <see cref="ProductsController"/>.
        /// </summary>
        /// <param name="context">Database context.</param>
        /// <param name="env">Hosting environment.</param>
        public ProductsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        /// <summary>
        /// Retrieves all products.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        /// <summary>
        /// Retrieves a product by its ID.
        /// </summary>
        /// <param name="id">Product identifier.</param>
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });

            return Ok(product);
        }

        /// <summary>
        /// Creates a new product (admin only).
        /// </summary>
        /// <param name="dto">Product data transfer object.</param>
        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<IActionResult> CreateProduct([FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = new Product
            {
                Name        = dto.Name,
                Description = dto.Description,
                Price       = dto.Price,
                Stock       = dto.Stock,
                ImageUrl    = dto.ImageUrl
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        /// <summary>
        /// Updates an existing product (admin only).
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <param name="dto">Updated product data.</param>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });

            product.Name        = dto.Name;
            product.Description = dto.Description;
            product.Price       = dto.Price;
            product.Stock       = dto.Stock;
            product.ImageUrl    = dto.ImageUrl;

            await _context.SaveChangesAsync();
            return Ok(product);
        }

        /// <summary>
        /// Deletes a product by ID (admin only).
        /// </summary>
        /// <param name="id">Product identifier.</param>
        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        /// <summary>
        /// Uploads an image and generates a 600×337 thumbnail (admin only).
        /// </summary>
        /// <param name="file">Image file to upload.</param>
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fichier invalide" });

            // Prepare directories
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            // Save original
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // Generate thumbnail
            var thumbDir = Path.Combine(uploadsDir, "thumbs");
            if (!Directory.Exists(thumbDir))
                Directory.CreateDirectory(thumbDir);

            using (Image image = Image.Load(filePath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size     = new Size(600, 337),
                    Mode     = ResizeMode.Pad,
                    Position = AnchorPositionMode.Center,
                    PadColor = Color.White
                }));

                var thumbPath = Path.Combine(thumbDir, fileName);
                image.Save(thumbPath, new JpegEncoder { Quality = 85 });
            }

            var baseUrl      = $"{Request.Scheme}://{Request.Host}";
            var thumbnailUrl = $"{baseUrl}/uploads/thumbs/{fileName}";
            return Ok(new { imageUrl = thumbnailUrl });
        }

        /// <summary>
        /// Updates stock of a product by a delta value (admin only).
        /// </summary>
        /// <param name="id">Product identifier.</param>
        /// <param name="delta">Change in stock quantity.</param>
        [Authorize(Roles = "admin")]
        [HttpPut("{id}/stock")]
        public async Task<IActionResult> UpdateStock(int id, [FromBody] int delta)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });

            product.Stock += delta;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Stock mis à jour", stock = product.Stock });
        }
    }
}
