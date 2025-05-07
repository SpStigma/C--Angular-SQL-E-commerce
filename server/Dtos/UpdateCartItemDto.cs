using System.ComponentModel.DataAnnotations;

namespace server.Dtos
{
    public class UpdateCartItemDto
    {
        [Required]
        public int ProductId { get; set; }
        [Range(1, 100)]
        public int Quantity { get; set; }
    }
}
