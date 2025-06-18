using Microsoft.AspNetCore.Http;

namespace EtherApp.Shared.ViewModels.Settings
{
    public class UpdateProfilePictureVM
    {
        public IFormFile ProfilePicture { get; set; }
    }
}
