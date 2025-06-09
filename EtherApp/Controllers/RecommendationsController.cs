using EtherApp.Controllers.Base;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Recommendations;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public class RecommendationsController : BaseController
{
    private readonly IPostsService _postsService;
    private readonly IInterestService _interestService;

    public RecommendationsController(IPostsService postsService, IInterestService interestService)
    {
        _postsService = postsService;
        _interestService = interestService;
    }

    public async Task<IActionResult> Index(bool filterByMyInterests = false, List<int> interests = null)
    {
        // Get current user ID
        var userId = GetUserId() ?? 0;
        if (userId == 0)
            return RedirectToAction("Login", "Account");
            
        // Get all available interests for filtering
        var allInterests = await _interestService.GetAllInterestsAsync();
        
        // Get user interests if filtering by them
        List<Post> recommendedPosts;
        
        if (filterByMyInterests)
        {
            // Use user's own interests for filtering
            var userInterests = await _interestService.GetUserInterestsAsync(userId);
            var userInterestIds = userInterests.Select(i => i.Id).ToList();
            recommendedPosts = await GetPostsByInterestIdsAsync(userId, userInterestIds);
        }
        else if (interests != null && interests.Any())
        {
            // Filter by selected interests
            recommendedPosts = await GetPostsByInterestIdsAsync(userId, interests);
        }
        else
        {
            // No filters - get default recommendations
            recommendedPosts = await _postsService.GetRecommendedPostsAsync(userId);
        }
        
        // Get similar users with shared interests
        var similarUsersWithInterests = await _interestService.GetSimilarUsersWithInterestsAsync(userId);
        
        // Map to view model
        var viewModel = new RecommendationsVM
        {
            RecommendedPosts = recommendedPosts, // Use Post directly instead of PostVM
            SimilarUsers = similarUsersWithInterests.Select(x => new UserMatchVM
            {
                User = x.User,
                Similarity = (int)x.Similarity,
                SharedInterests = x.SharedInterests
            }).ToList(),
            AvailableInterests = allInterests,
            SelectedInterests = interests ?? new List<int>(),
            FilterByMyInterests = filterByMyInterests
        };
        
        return View(viewModel);
    }
    
    // Helper method to get posts by interest IDs
    private async Task<List<Post>> GetPostsByInterestIdsAsync(int userId, List<int> interestIds)
    {
        if (interestIds == null || !interestIds.Any())
            return new List<Post>();
            
        // Get all posts that have at least one of the specified interests
        // Sort by relevance (how many matching interests they have)
        var posts = await _postsService.GetAllPostsAsync(userId);
        
        return posts
            .Where(p => !p.IsPrivate && p.UserId != userId)
            .Where(p => p.Interests != null && p.Interests.Any(i => interestIds.Contains(i.InterestId)))
            .OrderByDescending(p => p.Interests.Count(i => interestIds.Contains(i.InterestId)))
            .ThenByDescending(p => p.DateCreated)
            .ToList();
    }
}