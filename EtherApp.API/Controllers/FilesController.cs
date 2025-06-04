using EtherApp.Data.Helpers.Enums;
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
    public class FilesController : ControllerBase
    {
        private readonly IFilesService _filesService;
        private readonly UserManager<User> _userManager;

        public FilesController(
            IFilesService filesService,
            UserManager<User> userManager)
        {
            _filesService = filesService;
            _userManager = userManager;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (dto.File == null)
                return BadRequest("No file was provided");

            var imageUrl = await _filesService.UploadImageAsync(dto.File, dto.FileType);
            return Ok(new { ImageUrl = imageUrl });
        }
    }

    public class UploadFileDto
    {
        public IFormFile? File { get; set; }
        public ImageFileType FileType { get; set; }
    }
}