using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class UserInterest
    {
        public int UserId { get; set; }
        public User User { get; set; }

        public int InterestId { get; set; }
        public Interest Interest { get; set; }

        public double Weight { get; set; } // 0-1 representing strength of interest
        public DateTime DateAdded { get; set; }
    }
}
