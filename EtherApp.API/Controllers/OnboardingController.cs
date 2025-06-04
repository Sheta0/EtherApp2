using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class OnboardingController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly IInterestService _interestService;

        public OnboardingController(
            UserManager<User> userManager,
            IInterestService interestService)
        {
            _userManager = userManager;
            _interestService = interestService;
        }

        [HttpGet("status")]
        public async Task<IActionResult> GetOnboardingStatus()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Check if user already has interests
            var userInterests = await _interestService.GetUserInterestsAsync(user.Id);
            bool isOnboardingComplete = userInterests.Any();

            return Ok(new { isOnboardingComplete });
        }

        [HttpGet("interests")]
        public async Task<IActionResult> GetInterestsForOnboarding()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            // Get all available interests for onboarding
            var allInterests = await _interestService.GetAllInterestsAsync();
            
            return Ok(allInterests);
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteOnboarding([FromBody] CompleteOnboardingDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (dto.InterestIds == null || !dto.InterestIds.Any())
                return BadRequest("Please select at least one interest");

            var result = await _interestService.UpdateUserInterestsAsync(user.Id, dto.InterestIds);
            
            return Ok(new { success = result });
        }
    }

    public class CompleteOnboardingDto
    {
        public List<int> InterestIds { get; set; } = new List<int>();
    }
}