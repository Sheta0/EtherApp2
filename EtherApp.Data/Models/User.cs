using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class User : IdentityUser<int>
    {
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? Bio { get; set; }
        public bool IsDeleted { get; set; }

        // Navigation Properties
        public ICollection<Post> Posts { get; set; } = new List<Post>();
        public ICollection<Story> Stories { get; set; } = new List<Story>();
        public ICollection<Like> Like { get; set; } = new List<Like>();
        public ICollection<Comment> Comment { get; set; } = new List<Comment>();
        public ICollection<Favorite> Favorites { get; set; } = new List<Favorite>();
        public ICollection<Report> Reports { get; set; } = new List<Report>();
        public ICollection<UserHashtag> UserHashtags { get; set; } = new List<UserHashtag>();

        public ICollection<UserInterest> Interests { get; set; } = new List<UserInterest>();
    }
}
