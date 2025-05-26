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
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext      _context;
        private readonly IWebHostEnvironment _env;

        public ProductsController(AppDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env     = env;
        }

        // GET api/products
        [HttpGet]
        public async Task<IActionResult> GetProducts()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        // GET api/products/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound(new { message = "Produit introuvable" });

            return Ok(product);
        }

        // POST api/products
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

        // PUT api/products/{id}
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

        // DELETE api/products/{id}
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

        // POST api/products/upload
        // Upload d’image + génération d’une miniature 600×337 (ratio 16:9)
        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Fichier invalide" });

            // 1) Prépare les répertoires
            var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads");
            if (!Directory.Exists(uploadsDir))
                Directory.CreateDirectory(uploadsDir);

            var fileName = Guid.NewGuid() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            // 2) Enregistre l’original
            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            // 3) Génère la miniature
            var thumbDir = Path.Combine(uploadsDir, "thumbs");
            if (!Directory.Exists(thumbDir))
                Directory.CreateDirectory(thumbDir);

            // Charge l’image depuis le chemin, crop & resize, puis sauvegarde en 600×337
            using (Image image = Image.Load(filePath))
            {
                image.Mutate(x => x.Resize(new ResizeOptions
                {
                    Size    = new Size(600, 337),
                    Mode    = ResizeMode.Pad,
                    Position= AnchorPositionMode.Center,     // centre l’image dans le padding
                    PadColor= Color.LightGray                // couleur de fond (change selon ton UI)
                }));

                var thumbPath = Path.Combine(thumbDir, fileName);
                // Sauvegarde en JPEG (tu peux ajuster la qualité via JpegEncoder si besoin)
                image.Save(thumbPath, new JpegEncoder { Quality = 85 });
            }

            // 4) Renvoie l’URL publique de la miniature
            var baseUrl      = $"{Request.Scheme}://{Request.Host}";
            var thumbnailUrl = $"{baseUrl}/uploads/thumbs/{fileName}";
            return Ok(new { imageUrl = thumbnailUrl });
        }

        // PUT api/products/{id}/stock
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