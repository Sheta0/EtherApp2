// Services/InterestService.cs
using EtherApp.Data;
using EtherApp.Data.Models;
using EtherApp.Data.Services;
using EtherApp.Data.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

public class InterestService : IInterestService
{
    private readonly AppDBContext _context;
    private readonly IContentAnalysisService _contentAnalysis;

    public InterestService(AppDBContext context, IContentAnalysisService contentAnalysis)
    {
        _context = context;
        _contentAnalysis = contentAnalysis;
    }

    public async Task<List<Interest>> GetAllInterestsAsync()
    {
        return await _context.Interests.ToListAsync();
    }

    public async Task<List<Interest>> GetUserInterestsAsync(int userId)
    {
        return await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .Include(ui => ui.Interest)
            .Select(ui => ui.Interest)
            .ToListAsync();
    }

    public async Task<bool> UpdateUserInterestsAsync(int userId, List<int> interestIds)
    {
        // Get current user interests
        var currentInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .ToListAsync();

        // Remove interests that are no longer selected
        var interestsToRemove = currentInterests
            .Where(ui => !interestIds.Contains(ui.InterestId))
            .ToList();

        _context.UserInterests.RemoveRange(interestsToRemove);

        // Add new interests
        var existingInterestIds = currentInterests.Select(ui => ui.InterestId).ToList();
        var interestsToAdd = interestIds
            .Where(id => !existingInterestIds.Contains(id))
            .Select(id => new UserInterest
            {
                UserId = userId,
                InterestId = id,
                Weight = 1.0, // Initial weight
                DateAdded = DateTime.Now
            })
            .ToList();

        await _context.UserInterests.AddRangeAsync(interestsToAdd);

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ProcessPostInterestsAsync(int postId, string content)
    {
        var interestScores = await _contentAnalysis.AnalyzeContentAsync(content);

        // Remove existing post interest mappings
        var existingMappings = await _context.PostInterests
            .Where(pi => pi.PostId == postId)
            .ToListAsync();

        _context.PostInterests.RemoveRange(existingMappings);

        // Add new mappings
        var newMappings = interestScores.Select(score => new PostInterest
        {
            PostId = postId,
            InterestId = score.InterestId,
            Score = score.Score
        }).ToList();

        await _context.PostInterests.AddRangeAsync(newMappings);
        await _context.SaveChangesAsync();
    }

    // In InterestService.cs - update the UpdateUserInterestWeightsAsync method

    public async Task UpdateUserInterestWeightsAsync(int userId, int postId, InteractionType interactionType)
    {
        // Get post interests
        var postInterests = await _context.PostInterests
            .Where(pi => pi.PostId == postId)
            .ToListAsync();

        if (!postInterests.Any()) return;

        // Weight factors based on interaction type
        double weightFactor = interactionType switch
        {
            InteractionType.View => 0.05,
            InteractionType.Like => 0.2,
            InteractionType.Comment => 0.3,
            InteractionType.Favorite => 0.4,
            InteractionType.Create => 0.6, // Higher weight for content creation
            _ => 0.1
        };

        // Update user interest weights
        foreach (var postInterest in postInterests)
        {
            var userInterest = await _context.UserInterests
                .FirstOrDefaultAsync(ui =>
                    ui.UserId == userId &&
                    ui.InterestId == postInterest.InterestId);

            if (userInterest == null)
            {
                // Create new user interest if it doesn't exist
                userInterest = new UserInterest
                {
                    UserId = userId,
                    InterestId = postInterest.InterestId,
                    Weight = postInterest.Score * weightFactor,
                    DateAdded = DateTime.Now
                };

                await _context.UserInterests.AddAsync(userInterest);
            }
            else
            {
                // Update existing interest weight using exponential moving average
                userInterest.Weight = (userInterest.Weight * 0.7) + (postInterest.Score * weightFactor * 0.3);
                _context.UserInterests.Update(userInterest);
            }
        }

        await _context.SaveChangesAsync();
    }

    public async Task<double> CalculateInterestSimilarityAsync(int userId1, int userId2)
    {
        var user1Interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId1)
            .ToDictionaryAsync(ui => ui.InterestId, ui => ui.Weight);

        var user2Interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId2)
            .ToDictionaryAsync(ui => ui.InterestId, ui => ui.Weight);

        if (!user1Interests.Any() || !user2Interests.Any())
            return 0;

        // Get all unique interest IDs
        var allInterestIds = user1Interests.Keys.Union(user2Interests.Keys).ToList();

        double dotProduct = 0;
        double norm1 = 0;
        double norm2 = 0;

        // Calculate cosine similarity
        foreach (var interestId in allInterestIds)
        {
            double w1 = user1Interests.GetValueOrDefault(interestId, 0);
            double w2 = user2Interests.GetValueOrDefault(interestId, 0);

            dotProduct += w1 * w2;
            norm1 += w1 * w1;
            norm2 += w2 * w2;
        }

        if (norm1 == 0 || norm2 == 0)
            return 0;

        double similarity = dotProduct / (Math.Sqrt(norm1) * Math.Sqrt(norm2));

        // Convert to percentage
        return Math.Round(similarity * 100);
    }

    public async Task<List<(User User, double Similarity)>> GetSimilarUsersAsync(int userId, int count = 5)
    {
        var allUsers = await _context.Users
            .Where(u => u.Id != userId)
            .ToListAsync();

        var result = new List<(User User, double Similarity)>();

        foreach (var user in allUsers)
        {
            var similarity = await CalculateInterestSimilarityAsync(userId, user.Id);
            result.Add((user, similarity));
        }

        return result
            .OrderByDescending(u => u.Similarity)
            .Take(count)
            .ToList();
    }
}

public enum InteractionType
{
    View,
    Like,
    Comment,
    Favorite,
    Create 
}
