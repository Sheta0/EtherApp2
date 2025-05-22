using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class Story
    {
        public int Id { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime DateCreated { get; set; }
        public bool IsDeleted { get; set; }

        //foreign key
        public int UserId { get; set; }

        //Navigation properties
        public User User { get; set; }

    }
}
