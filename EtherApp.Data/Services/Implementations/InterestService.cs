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

    public async Task UpdateUserInterestWeightsAsync(int userId, int postId, InteractionType interactionType)
    {
        // Get post interests - consider including the Interest entity for better context
        var postInterests = await _context.PostInterests
            .Where(pi => pi.PostId == postId)
            .ToListAsync();

        if (!postInterests.Any()) return;

        // Get all user's interests in one query instead of querying for each interest
        var userInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId && postInterests.Select(pi => pi.InterestId).Contains(ui.InterestId))
            .ToDictionaryAsync(ui => ui.InterestId, ui => ui);

        // Weight factors based on interaction type
        double weightFactor = interactionType switch
        {
            InteractionType.View => 0.05,
            InteractionType.Like => 0.2,
            InteractionType.Comment => 0.3,
            InteractionType.Favorite => 0.4,
            InteractionType.Create => 0.6,
            _ => 0.1
        };

        // Batch the changes instead of making individual queries
        var interestsToAdd = new List<UserInterest>();

        foreach (var postInterest in postInterests)
        {
            if (userInterests.TryGetValue(postInterest.InterestId, out var userInterest))
            {
                // Normalize weights to prevent unbounded growth - cap at 5.0
                userInterest.Weight = Math.Min(5.0, (userInterest.Weight * 0.7) + (postInterest.Score * weightFactor * 0.3));
            }
            else
            {
                interestsToAdd.Add(new UserInterest
                {
                    UserId = userId,
                    InterestId = postInterest.InterestId,
                    Weight = Math.Min(1.0, postInterest.Score * weightFactor), // Capped at 1.0 initially
                    DateAdded = DateTime.Now
                });
            }
        }

        if (interestsToAdd.Any())
        {
            await _context.UserInterests.AddRangeAsync(interestsToAdd);
        }

        await _context.SaveChangesAsync();
    }

    public async Task<double> CalculateInterestSimilarityAsync(int userId1, int userId2)
    {
        // Fetch both users' interests in a single query for better performance
        var allInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId1 || ui.UserId == userId2)
            .ToListAsync();
            
        var user1Interests = allInterests
            .Where(ui => ui.UserId == userId1)
            .ToDictionary(ui => ui.InterestId, ui => ui.Weight);
            
        var user2Interests = allInterests
            .Where(ui => ui.UserId == userId2)
            .ToDictionary(ui => ui.InterestId, ui => ui.Weight);

        if (!user1Interests.Any() || !user2Interests.Any())
            return 0;

        // Shared interests should matter more than just having similar profiles
        var sharedInterestIds = user1Interests.Keys.Intersect(user2Interests.Keys).ToList();
        var sharedInterestCount = sharedInterestIds.Count;
        var sharedInterestBonus = sharedInterestCount > 0 ? Math.Min(0.2, sharedInterestCount * 0.05) : 0;
        
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
        
        // Add bonus for shared interests and convert to percentage
        similarity = Math.Min(1.0, similarity + sharedInterestBonus);
        return Math.Round(similarity * 100);
    }

    public async Task<List<(User User, double Similarity)>> GetSimilarUsersAsync(int userId, int count = 5)
    {
        // Load the current user's interests first
        var userInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId)
            .ToListAsync();
            
        if (!userInterests.Any())
        {
            return new List<(User User, double Similarity)>();
        }
        
        // Get IDs of friends to exclude
        var friendIds = await _context.Friendships
            .Where(f => f.SenderId == userId || f.ReceiverId == userId)
            .Select(f => f.SenderId == userId ? f.ReceiverId : f.SenderId)
            .ToListAsync();
        
        // Find users who share at least one interest (much more efficient)
        var interestIds = userInterests.Select(ui => ui.InterestId).ToList();
        var potentialSimilarUserIds = await _context.UserInterests
            .Where(ui => ui.UserId != userId && 
                        !friendIds.Contains(ui.UserId) && // Exclude friends
                        interestIds.Contains(ui.InterestId))
            .Select(ui => ui.UserId)
            .Distinct()
            .Take(20) // Reasonable limit for efficiency
            .ToListAsync();
            
        var potentialSimilarUsers = await _context.Users
            .Where(u => potentialSimilarUserIds.Contains(u.Id))
            .ToListAsync();
            
        var result = new List<(User User, double Similarity)>();

        foreach (var user in potentialSimilarUsers)
        {
            var similarity = await CalculateInterestSimilarityAsync(userId, user.Id);
            if (similarity > 0)  // Only include users with some similarity
            {
                result.Add((user, similarity));
            }
        }

        // If we don't have enough similar users through shared interests,
        // get some additional users (who are not friends) to meet the requested count
        if (result.Count < count)
        {
            var additionalUsersNeeded = count - result.Count;
            var existingUserIds = result.Select(r => r.User.Id).ToList();
            existingUserIds.Add(userId); // Exclude current user too
            existingUserIds.AddRange(friendIds); // Exclude friends
            
            var additionalUsers = await _context.Users
                .Where(u => !existingUserIds.Contains(u.Id))
                .Take(additionalUsersNeeded)
                .ToListAsync();
                
            foreach (var user in additionalUsers)
            {
                var similarity = await CalculateInterestSimilarityAsync(userId, user.Id);
                result.Add((user, similarity));
            }
        }

        return result
            .OrderByDescending(u => u.Similarity)
            .Take(count)
            .ToList();
    }

    public async Task<List<Interest>> GetSharedInterestsAsync(int userId1, int userId2)
    {
        var user1Interests = await _context.UserInterests
            .Where(ui => ui.UserId == userId1)
            .Select(ui => ui.InterestId)
            .ToListAsync();
            
        var sharedInterests = await _context.UserInterests
            .Where(ui => ui.UserId == userId2 && user1Interests.Contains(ui.InterestId))
            .Include(ui => ui.Interest)
            .Select(ui => ui.Interest)
            .ToListAsync();
            
        return sharedInterests;
    }

    // Enhancement to GetSimilarUsersAsync to include shared interests
    public async Task<List<(User User, double Similarity, List<Interest> SharedInterests)>> GetSimilarUsersWithInterestsAsync(int userId, int count = 5)
    {
        // Reuse existing code to get similar users 
        var similarUsers = await GetSimilarUsersAsync(userId, count);
        
        // Enhance with shared interests
        var result = new List<(User User, double Similarity, List<Interest> SharedInterests)>();
        
        foreach (var (user, similarity) in similarUsers)
        {
            var sharedInterests = await GetSharedInterestsAsync(userId, user.Id);
            result.Add((user, similarity, sharedInterests));
        }
        
        return result;
    }

    public async Task<List<User>> GetUsersWithInterestsAsync(List<int> interestIds)
    {
        return await _context.UserInterests
            .Where(ui => interestIds.Contains(ui.InterestId))
            .Select(ui => ui.User)
            .Distinct()
            .ToListAsync();
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
