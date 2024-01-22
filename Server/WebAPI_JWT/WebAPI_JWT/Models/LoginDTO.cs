using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace WebAPI_JWT.Models
{
    public class LoginDto
    {
        [Required(ErrorMessage = "Password is required")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email is required")]
        public string? Email { get; set; }

        public string TwoFactorCode { get; set; } = "string";

        public string TwoFactorRecoveryCode { get; set; } = "string";
    }
}
