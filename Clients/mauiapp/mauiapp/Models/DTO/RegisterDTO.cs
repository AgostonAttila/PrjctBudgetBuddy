using mauiapp.Validation;
using System.ComponentModel.DataAnnotations;

namespace mauiapp.Models.DTO
{
    public class RegisterDTO
    {

        [Required]
        public string? FirstName { get; set; }

		[Required]
		public string? LastName { get; set; }

		[Required(ErrorMessage = "Password is required")]
        [StringLength(50, ErrorMessage = "Must be between 10 and 50 characters", MinimumLength = 10)]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

		[Required(ErrorMessage = "Confirm Password is required")]
		[Compare(nameof(Password))]
		[StringLength(50, ErrorMessage = "Must be between 10 and 50 characters", MinimumLength = 10)]
		[DataType(DataType.Password)]
		public string? ConfirmPassword { get; set; }

		[Required(ErrorMessage = "Email is required")]
		[EmailAddress]
		//[RegularExpression("^[a-zA-Z0-9_.-]+@[a-zA-Z0-9-]+.[a-zA-Z0-9-.]+$", ErrorMessage = "Must be a valid email")]
        public string? Email { get; set; }

		[IsTrue(ErrorMessage = "AcceptTerms must be checked")]
		[Required]
		public bool AcceptTerms { get; set; }
	}
}
