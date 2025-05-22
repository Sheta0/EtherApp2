using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class Interest
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string IconName { get; set; } // For UI display

        // Navigation properties
        public ICollection<UserInterest> UserInterests { get; set; }
        public ICollection<PostInterest> PostInterests { get; set; }
    }

}
