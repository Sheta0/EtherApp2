using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.Shared.ViewModels.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    public class AuthenticationController : BaseApiController
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;

        public AuthenticationController(
            UserManager<User> userManager, 
            SignInManager<User> signInManager,
            ITokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid model"));

            // Find user by email
            var existingUser = await _userManager.FindByEmailAsync(loginVM.Email);
            if (existingUser is null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid email or password"));

            // Check password
            var result = await _signInManager.CheckPasswordSignInAsync(
                existingUser, loginVM.Password, lockoutOnFailure: false);

            if (!result.Succeeded)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid email or password"));

            // Generate JWT token
            var token = await _tokenService.GenerateJwtTokenAsync(existingUser);

            // Return user info and token
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Token = token,
                UserId = existingUser.Id,
                Username = existingUser.UserName,
                Email = existingUser.Email,
                FullName = existingUser.FullName
            }));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return BadRequest(ApiResponse<object>.ErrorResponse("Invalid model"));

            // Check if email already exists
            var existingUser = await _userManager.FindByEmailAsync(registerVM.Email);
            if (existingUser is not null)
                return BadRequest(ApiResponse<object>.ErrorResponse("Email already exists"));

            // Create new user
            var user = new User
            {
                UserName = registerVM.Email,
                Email = registerVM.Email,
                FullName = $"{registerVM.FirstName} {registerVM.LastName}"
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    string.Join(", ", result.Errors.Select(e => e.Description))));
            }

            // Add user to User role
            await _userManager.AddToRoleAsync(user, AppRoles.User);
            
            // Generate token
            var token = await _tokenService.GenerateJwtTokenAsync(user);

            // Return user info and token
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                Token = token,
                UserId = user.Id,
                Username = user.UserName,
                Email = user.Email,
                FullName = user.FullName
            }));
        }

        [HttpPost("logout")]
        [Authorize]
        public IActionResult Logout()
        {
            return Ok(ApiResponse<object>.SuccessResponse("Logged out successfully"));
        }
    }
}