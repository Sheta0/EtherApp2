using EtherApp.Data.Models;
using EtherApp.Shared.ViewModels.Home;
using System.Collections.Generic;

namespace EtherApp.Shared.ViewModels.Recommendations
{
    public class RecommendationsVM
    {
        public List<Post> RecommendedPosts { get; set; } = new List<Post>();
        public List<UserMatchVM> SimilarUsers { get; set; } = new List<UserMatchVM>();

        // Properties for filtering
        public List<Interest> AvailableInterests { get; set; } = new List<Interest>();
        public List<int> SelectedInterests { get; set; } = new List<int>();
        public bool FilterByMyInterests { get; set; }
    }   

    // Include other view models referenced by RecommendationsVM
    public class UserMatchVM
    {
        public User User { get; set; }
        public int Similarity { get; set; }
        public List<Interest> SharedInterests { get; set; } = new List<Interest>();
    }
}
