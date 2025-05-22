using EtherApp.Data.Models;

namespace EtherApp.ViewModels.Recommendations
{
    public class RecommendationsVM
    {
        public List<Post> RecommendedPosts { get; set; } = new();
        public List<User> SimilarUsers { get; set; } = new();
    }
}
