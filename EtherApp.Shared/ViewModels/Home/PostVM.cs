using Microsoft.AspNetCore.Http;

namespace EtherApp.Shared.ViewModels.Home
{
    public class PostVM
    {
        public string Content { get; set; }
        public IFormFile Image { get; set; }
        public bool? IsPrivate { get; set; } = false;
    }
}
