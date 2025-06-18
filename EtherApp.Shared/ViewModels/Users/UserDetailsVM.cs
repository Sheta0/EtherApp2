using EtherApp.Data.Models;

namespace EtherApp.Shared.ViewModels.Users
{
    internal class UserDetailsVM
    {
        public User? User { get; set; }
        public List<Post>? Posts { get; set; }
        public List<Interest>? Interests { get; set; }
        public double SimilarityPercentage { get; set; }
        public bool IsCurrentUser { get; set; }
    }
}