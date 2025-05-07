using System.ComponentModel.DataAnnotations;

namespace server.Dtos
{
    public class UserDto
    {
        [Required]
        [MinLength(3)]
        public string? Username { get; set; }

        [Required]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(6)]
        public string? Password { get; set; }
    }
}
