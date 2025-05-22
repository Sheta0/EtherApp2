using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Models
{
    public class PostInterest
    {
        public int PostId { get; set; }
        public Post Post { get; set; }

        public int InterestId { get; set; }
        public Interest Interest { get; set; }

        public double Score { get; set; } // Relevance score of post to interest
    }
}
