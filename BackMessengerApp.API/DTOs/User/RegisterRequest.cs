using System.ComponentModel.DataAnnotations;
namespace BackMessengerApp.API.DTOs.User
{
    public class RegisterRequest
    {
        [Required]
        [StringLength(25, MinimumLength = 2, ErrorMessage = "Name would be between from {2} to {1} symbols")]
        public string Name { get; set; }
        [Required]
        [EmailAddress]
        public string Email { get; set; } 

        [Required]
        public string Password { get; set; }    
    }
}
