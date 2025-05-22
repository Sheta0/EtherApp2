using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using EtherApp.Data.Models;

namespace EtherApp.Data.Services.Interfaces
{
    public interface IInterestService
    {
        Task<List<Interest>> GetAllInterestsAsync();
        Task<List<Interest>> GetUserInterestsAsync(int userId);
        Task<bool> UpdateUserInterestsAsync(int userId, List<int> interestIds);
        Task ProcessPostInterestsAsync(int postId, string content);
        Task UpdateUserInterestWeightsAsync(int userId, int postId, InteractionType interactionType);
        Task<double> CalculateInterestSimilarityAsync(int userId1, int userId2);
        Task<List<(User User, double Similarity)>> GetSimilarUsersAsync(int userId, int count = 5);
    }
}
