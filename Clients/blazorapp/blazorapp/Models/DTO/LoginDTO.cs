using System.ComponentModel.DataAnnotations;

namespace blazorapp.Models.DTO
{
    public class LoginDTO
    {
       
        [Required(ErrorMessage = "Password is required")]    
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email is required")]     
        public string Email { get; set; }   
    }
}
