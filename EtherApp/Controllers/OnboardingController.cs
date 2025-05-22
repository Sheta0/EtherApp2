using EtherApp.Data.Models;
using EtherApp.ViewModels.Users;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

public class OnboardingController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly InterestService _interestService;

    public OnboardingController(
        UserManager<User> userManager,
        InterestService interestService)
    {
        _userManager = userManager;
        _interestService = interestService;
    }

    [HttpGet]
    public async Task<IActionResult> Welcome()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        // Check if user already has interests
        var userInterests = await _interestService.GetUserInterestsAsync(user.Id);
        if (userInterests.Any())
        {
            return RedirectToAction("Index", "Home");
        }

        var allInterests = await _interestService.GetAllInterestsAsync();

        var viewModel = new EditInterestsViewModel
        {
            AllInterests = allInterests,
            SelectedInterestIds = new List<int>()
        };

        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Welcome(EditInterestsViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }

        await _interestService.UpdateUserInterestsAsync(user.Id, model.SelectedInterestIds);

        return RedirectToAction("Index", "Home");
    }
}
