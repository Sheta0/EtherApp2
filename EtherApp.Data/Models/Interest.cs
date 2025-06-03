using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class Interest
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string IconName { get; set; } = string.Empty;

        // Additional fields for improved analysis
        public string Keywords { get; set; } = string.Empty; // Comma-separated keywords

        // Navigation properties
        public ICollection<UserInterest> UserInterests { get; set; } = new List<UserInterest>();
        public ICollection<PostInterest> PostInterests { get; set; } = new List<PostInterest>();
    }

}
