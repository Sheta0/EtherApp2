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
    public class FriendsController : ControllerBase
    {
        private readonly IFriendsService _friendsService;
        private readonly UserManager<User> _userManager;

        public FriendsController(
            IFriendsService friendsService,
            UserManager<User> userManager)
        {
            _friendsService = friendsService;
            _userManager = userManager;
        }

        [HttpGet("suggested")]
        public async Task<IActionResult> GetSuggestedFriends()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var suggestedFriends = await _friendsService.GetSuggestedFriendsAsync(user.Id);
            return Ok(suggestedFriends);
        }

        [HttpGet]
        public async Task<IActionResult> GetUserFriends()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var friends = await _friendsService.GetUserFriendsAsync(user.Id);
            return Ok(friends);
        }

        [HttpGet("requests/sent")]
        public async Task<IActionResult> GetSentRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var sentRequests = await _friendsService.GetSentFriendRequestsAsync(user.Id);
            return Ok(sentRequests);
        }

        [HttpGet("requests/received")]
        public async Task<IActionResult> GetReceivedRequests()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var receivedRequests = await _friendsService.GetReceivedFriendRequestsAsync(user.Id);
            return Ok(receivedRequests);
        }

        [HttpPost("request/{receiverId}")]
        public async Task<IActionResult> SendFriendRequest(int receiverId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _friendsService.SendRequestAsync(user.Id, receiverId);
            return Ok();
        }

        [HttpPut("request/{requestId}")]
        public async Task<IActionResult> UpdateFriendRequest(int requestId, [FromBody] UpdateRequestDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _friendsService.UpdateRequestAsync(requestId, dto.Status);
            return Ok(result);
        }

        [HttpDelete("{friendshipId}")]
        public async Task<IActionResult> RemoveFriend(int friendshipId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _friendsService.RemoveFriendAsync(friendshipId);
            return Ok();
        }
    }

    public class UpdateRequestDto
    {
        public string Status { get; set; } = string.Empty;
    }
}