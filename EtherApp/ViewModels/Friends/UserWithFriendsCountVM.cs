using EtherApp.Data.Models;

namespace EtherApp.ViewModels.Friends
{
    public class UserWithFriendsCountVM
    {
        public int UserId { get; set; }
        public string FullName { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public int FriendsCount { get; set; }
        public FriendRequest PendingFriendRequest { get; set; }
        public bool HasSentRequest { get; set; }

        public string FriendsCountText => FriendsCount == 1 ? "1 friend" : $"{FriendsCount} friends";
    }

}
