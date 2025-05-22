using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public string Content { get; set; }

        public DateTime DateCreated { get; set; }
        public DateTime DateUpdated { get; set; }   

        // Foreign Keys
        public int UserId { get; set; }
        public int PostId { get; set; }

        // Navigation Properties
        public Post Post { get; set; }
        public User User { get; set; }
    }
}
