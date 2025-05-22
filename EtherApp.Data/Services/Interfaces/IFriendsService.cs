using EtherApp.Data.Dtos;
using EtherApp.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EtherApp.Data.Services.Interfaces
{
    public interface IFriendsService
    {
        Task SendRequestAsync(int senderId, int receiverId);
        Task<FriendRequest> UpdateRequestAsync(int requestId, string status);
        Task RemoveFriendAsync(int friendshipId);
        Task<List<UserWithFriendsCountDto>> GetSuggestedFriendsAsync(int userId);
        Task<List<FriendRequest>> GetSentFriendRequestsAsync(int userId);
        Task<List<FriendRequest>> GetReceivedFriendRequestsAsync(int userId);
        Task<List<Friendship>> GetUserFriendsAsync(int userId);
        Task<FriendRequest> GetFriendRequestAsync(int senderId, int receiverId);
        Task<int> GetFriendshipIdAsync(int userId1, int userId2);
        Task<bool> IsFriendAsync(int userId1, int userId2);
    }
}
