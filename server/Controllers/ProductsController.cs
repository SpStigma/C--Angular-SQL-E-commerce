using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using server.Dtos;
using server.Models;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly Data.AppDbContext _context;

        public ProductsController(Data.AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetAll()
        {
            var products = await _context.Products.ToListAsync();
            return Ok(products);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        public async Task<ActionResult<Product>> Create(ProductDto productDto)
        {
            var product = new Product
            {
                Name = productDto.Name,
                Description = productDto.Description,
                Price = productDto.Price,
                Stock = productDto.Stock,
                ImageUrl = productDto.ImageUrl // ← ajout ici
            };

            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
        }

        [Authorize(Roles = "admin")]
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, ProductDto productDto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            product.Name = productDto.Name;
            product.Description = productDto.Description;
            product.Price = productDto.Price;
            product.Stock = productDto.Stock;
            product.ImageUrl = productDto.ImageUrl; // ← ajout ici

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [Authorize(Roles = "admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
                return NotFound();

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("secure")]
        [Authorize]
        public IActionResult GetSecureData()
        {
            return Ok(new { message = "Tu es bien connecté" });
        }

        [HttpGet("public")]
        public IActionResult GetPublicData()
        {
            return Ok(new { message = "Donnée publique accessible à tous" });
        }

        [HttpGet("test-error")]
        public IActionResult TestError()
        {
            throw new Exception("Erreur de test simulée");
        }

        

        [Authorize(Roles = "admin")]
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "Fichier invalide" });
            }

            var uploadsDir = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
            if (!Directory.Exists(uploadsDir))
            {
                Directory.CreateDirectory(uploadsDir);
            }

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var filePath = Path.Combine(uploadsDir, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var baseUrl = $"{Request.Scheme}://{Request.Host}";
            var imageUrl = $"{baseUrl}/Uploads/{fileName}";

            return Ok(new { imageUrl });
        }

    }
}
