using EtherApp.Data.Models;

namespace EtherApp.ViewModels.Friends
{
    public class FriendshipVM
    {
        public List<FriendRequest> FriendRequests { get; set; } = new List<FriendRequest>();
        public List<FriendRequest> ReceivedRequests { get; set; } = new List<FriendRequest>();
        public List<Friendship> Friends { get; set; } = new List<Friendship>();
    }
}
