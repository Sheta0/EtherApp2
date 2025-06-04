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
    public class HashtagsController : ControllerBase
    {
        private readonly IHashtagsService _hashtagsService;
        private readonly UserManager<User> _userManager;

        public HashtagsController(
            IHashtagsService hashtagsService,
            UserManager<User> userManager)
        {
            _hashtagsService = hashtagsService;
            _userManager = userManager;
        }

        [HttpPost("process")]
        public async Task<IActionResult> ProcessHashtags([FromBody] ProcessHashtagsDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _hashtagsService.ProcessHashtagsForNewPostAsync(dto.Content, user.Id);
            return Ok();
        }

        [HttpDelete("remove")]
        public async Task<IActionResult> RemoveHashtags([FromBody] ProcessHashtagsDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(dto.Content, user.Id);
            return Ok();
        }
    }

    public class ProcessHashtagsDto
    {
        public string Content { get; set; } = string.Empty;
    }
}