using System.ComponentModel.DataAnnotations;

namespace EtherApp.ViewModels.Authentication
{
    public class RegisterVM
    {
        [Required(ErrorMessage = "First Name is Required!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 50")]
        [RegularExpression(@"^[a-zA-Z]+$", ErrorMessage = "First Name must contain only letters")]
        public string FirstName { get; set; }

        [Required(ErrorMessage = "Last Name is Required!")]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "First Name must be between 2 and 50")]
        public string LastName { get; set; }

        [Required(ErrorMessage = "Email is Required!")]
        [EmailAddress(ErrorMessage = "Invalid Email Address!")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is Required!")]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is Required!")]
        [Compare("Password", ErrorMessage = "Passwords do not match!")]
        public string ConfirmPassword { get; set; }
    }
}
