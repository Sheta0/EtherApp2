using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class Hashtag
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int Count { get; set; }
        public DateTime DateCreate { get; set; }
        public DateTime DateUpdate { get; set; }
        // Navigation Properties
        public ICollection<UserHashtag> UserHashtags { get; set; } = new List<UserHashtag>();
    }
}
