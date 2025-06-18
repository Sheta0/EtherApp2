using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace EtherApp.Shared.ViewModels.Users
{
    public class SetupProfileViewModel
    {
        [Required(ErrorMessage = "Username is required")]
        [RegularExpression(@"^[a-zA-Z0-9._-]{3,20}$", ErrorMessage = "Username must be 3-20 characters and can only contain letters, numbers, dots, underscores, and hyphens")]
        [Display(Name = "Username")]
        public string UserName { get; set; }
        
        [Display(Name = "Profile Picture")]
        public IFormFile? ProfilePicture { get; set; }
    }
}