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
    public class InterestsController : ControllerBase
    {
        private readonly IInterestService _interestService;
        private readonly UserManager<User> _userManager;

        public InterestsController(
            IInterestService interestService,
            UserManager<User> userManager)
        {
            _interestService = interestService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllInterests()
        {
            var interests = await _interestService.GetAllInterestsAsync();
            return Ok(interests);
        }

        [HttpGet("user")]
        public async Task<IActionResult> GetUserInterests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var interests = await _interestService.GetUserInterestsAsync(user.Id);
            return Ok(interests);
        }

        [HttpPut("user")]
        public async Task<IActionResult> UpdateUserInterests([FromBody] UpdateInterestsDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _interestService.UpdateUserInterestsAsync(user.Id, dto.InterestIds);
            return Ok(new { Success = result });
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> AnalyzeContent([FromBody] AnalyzeContentDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _interestService.ProcessPostInterestsAsync(dto.PostId, dto.Content);
            return Ok();
        }
    }

    public class UpdateInterestsDto
    {
        public List<int> InterestIds { get; set; } = new List<int>();
    }

    public class AnalyzeContentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}