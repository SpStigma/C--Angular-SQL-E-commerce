using System.ComponentModel.DataAnnotations;

namespace server.Dtos
{
    public class ProductDto
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue)]
        public decimal Price { get; set; }

        [Range(0, int.MaxValue)]
        public int Stock { get; set; }

        [StringLength(500)]
        public string? ImageUrl { get; set; } // <- ici
    }
}
