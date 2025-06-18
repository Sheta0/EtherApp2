using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

public class UserController(
    UserManager<User> userManager,
    IInterestService interestService,
    IPostsService postService) : Controller
{
    private readonly UserManager<User> _userManager = userManager;
    private readonly IInterestService _interestService = interestService;
    private readonly IPostsService _postService = postService;

    [HttpGet]
    public async Task<IActionResult> Details(int userId)
    {
        var profileUser = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (profileUser == null)
        {
            return NotFound();
        }

        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Challenge();
        }

        // Calculate similarity percentage
        double similarityPercentage = await _interestService.CalculateInterestSimilarityAsync(
            currentUser.Id, profileUser.Id);

        // Get user posts - use the new method that filters by user ID
        var userPosts = await _postService.GetUserPostsAsync(userId, currentUser.Id);

        // Get user interests
        var userInterests = await _interestService.GetUserInterestsAsync(userId);

        var viewModel = new UserDetailsVM
        {
            User = profileUser,
            Posts = userPosts,
            Interests = userInterests,
            SimilarityPercentage = similarityPercentage,
            IsCurrentUser = currentUser.Id == userId
        };

        return View(viewModel);
    }

    [HttpGet]
    public async Task<IActionResult> EditInterests()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        var allInterests = await _interestService.GetAllInterestsAsync();
        var userInterests = await _interestService.GetUserInterestsAsync(user.Id);

        var viewModel = new EditInterestsViewModel
        {
            AllInterests = allInterests,
            SelectedInterestIds = userInterests.Select(i => i.Id).ToList()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditInterests(EditInterestsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // Check if any interests are selected
        if (model.SelectedInterestIds == null || model.SelectedInterestIds.Count == 0)
        {
            ModelState.AddModelError("SelectedInterestIds", "You must select at least one interest");
            // Repopulate the model to redisplay the form
            model.AllInterests = await _interestService.GetAllInterestsAsync();
            return View(model);
        }

        await _interestService.UpdateUserInterestsAsync(user.Id, model.SelectedInterestIds);

        return RedirectToAction("Details", new { userId = user.Id });
    }

    [HttpGet]
    public async Task<JsonResult> GetUserRelatedPosts(int userId, int currentPostId)
    {
        var currentUser = await _userManager.GetUserAsync(User);
        if (currentUser == null)
        {
            return Json(new List<object>());
        }

        // Get user posts
        var userPosts = await _postService.GetUserPostsAsync(userId, currentUser.Id);
        
        // Filter out the current post and take only a few recent ones
        var relatedPosts = userPosts
            .Where(p => p.Id != currentPostId)
            .OrderByDescending(p => p.DateCreated)
            .Take(3)
            .Select(p => new
            {
                Id = p.Id,
                Title = !string.IsNullOrEmpty(p.Content) 
                    ? (p.Content.Length > 50 ? p.Content.Substring(0, 50) + "..." : p.Content) 
                    : "Post without text",
                CreatedDate = p.DateCreated,
                DaysAgo = (DateTime.Now - p.DateCreated).Days,
                HasImage = !string.IsNullOrEmpty(p.ImageUrl)
            })
            .ToList();

        return Json(relatedPosts);
    }

    [HttpGet]
    public async Task<JsonResult> GetUserStats(int userId)
    {
        var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return Json(new { success = false, message = "User not found" });
        }

        // Get post count
        var userPosts = await _postService.GetUserPostsAsync(userId, userId);
        var postCount = userPosts?.Count ?? 0;
        
        // Get friendship information using the FriendsService
        var friendsService = HttpContext.RequestServices.GetService<IFriendsService>();
        int friendsCount = 0;
        
        if (friendsService != null)
        {
            var userFriends = await friendsService.GetUserFriendsAsync(userId);
            friendsCount = userFriends.Count;
        }

        return Json(new { 
            success = true,
            postCount = postCount,
            friendsCount = friendsCount
        });
    }
}
