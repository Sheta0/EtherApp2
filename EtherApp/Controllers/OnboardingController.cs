using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.ViewModels.Users;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.IO;

public class OnboardingController : Controller
{
    private readonly UserManager<User> _userManager;
    private readonly IInterestService _interestService;
    private readonly IWebHostEnvironment _webHostEnvironment;

    public OnboardingController(
        UserManager<User> userManager,
        IInterestService interestService,
        IWebHostEnvironment webHostEnvironment)
    {
        _userManager = userManager;
        _interestService = interestService;
        _webHostEnvironment = webHostEnvironment;
    }

    [HttpGet]
    public async Task<IActionResult> SetupProfile()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }
        
        var viewModel = new SetupProfileViewModel
        {
            UserName = user.UserName
        };
        
        return View(viewModel);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetupProfile(SetupProfileViewModel model)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            return Challenge();
        }
        
        if (!ModelState.IsValid)
        {
            return View(model);
        }
        
        // Check if username is already taken
        var existingUser = await _userManager.FindByNameAsync(model.UserName);
        if (existingUser != null && existingUser.Id != user.Id)
        {
            ModelState.AddModelError("UserName", "Username is already taken");
            return View(model);
        }
        
        user.UserName = model.UserName;
        
        // Handle profile picture upload if provided
        if (model.ProfilePicture != null && model.ProfilePicture.Length > 0)
        {
            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.ProfilePicture.FileName);
            var uploadPath = Path.Combine("images", "avatar");
            var fullPath = Path.Combine(_webHostEnvironment.WebRootPath, uploadPath);
            
            if (!Directory.Exists(fullPath))
            {
                Directory.CreateDirectory(fullPath);
            }
            
            var filePath = Path.Combine(fullPath, fileName);
            using (var fileStream = new FileStream(filePath, FileMode.Create))
            {
                await model.ProfilePicture.CopyToAsync(fileStream);
            }
            
            user.ProfilePictureUrl = Path.Combine(uploadPath, fileName).Replace("\\", "/");
        }
        
        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError(string.Empty, error.Description);
            }
            return View(model);
        }
        
        return RedirectToAction("Welcome");
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
