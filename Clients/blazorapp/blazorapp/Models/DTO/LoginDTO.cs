using System.ComponentModel.DataAnnotations;

namespace blazorapp.Models.DTO
{
    public class LoginDTO
    {
       
        [Required(ErrorMessage = "Password is required")]    
        public string? Password { get; set; }

        [Required(ErrorMessage = "Email is required")]     
        public string Email { get; set; }

        public string TwoFactorCode { get; set; } = "string";

		public string TwoFactorRecoveryCode { get; set; } = "string";

	}
}
