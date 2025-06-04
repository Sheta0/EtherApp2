using EtherApp.Data.Helpers.Constants;
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
    public class PostsController : ControllerBase
    {
        private readonly IPostsService _postsService;
        private readonly IFilesService _filesService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IInterestService _interestService;
        private readonly INotificationService _notificationService;
        private readonly UserManager<User> _userManager;

        public PostsController(
            IPostsService postsService,
            IFilesService filesService,
            IHashtagsService hashtagsService,
            IInterestService interestService,
            INotificationService notificationService,
            UserManager<User> userManager)
        {
            _postsService = postsService;
            _filesService = filesService;
            _hashtagsService = hashtagsService;
            _interestService = interestService;
            _notificationService = notificationService;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var allPosts = await _postsService.GetAllPostsAsync(user.Id);
            return Ok(allPosts);
        }

        [HttpGet("recommended")]
        public async Task<IActionResult> GetRecommendedPosts([FromQuery] int count = 10)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var recommendedPosts = await _postsService.GetRecommendedPostsAsync(user.Id, count);
            return Ok(recommendedPosts);
        }

        [HttpGet("favorites")]
        public async Task<IActionResult> GetFavoritedPosts()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var favoritedPosts = await _postsService.GetAllFavoritedPostsAsync(user.Id);
            return Ok(favoritedPosts);
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserPosts(int userId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var userPosts = await _postsService.GetUserPostsAsync(userId, user.Id);
            if (userPosts == null)
                return NotFound();

            return Ok(userPosts);
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostDetails(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            return Ok(post);
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] CreatePostDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            if (string.IsNullOrWhiteSpace(dto.Content) && dto.Image == null)
            {
                return BadRequest("Please provide either text content or an image for your post.");
            }

            string imageUploadPath = null;
            if (dto.Image != null)
            {
                imageUploadPath = await _filesService.UploadImageAsync(dto.Image, ImageFileType.PostImage);
            }

            var newPost = new Post
            {
                Content = dto.Content ?? string.Empty,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = user.Id,
                IsPrivate = dto.IsPrivate
            };

            // Create the post
            var createdPost = await _postsService.CreatePostAsync(newPost);

            // Process hashtags if content is not empty
            if (!string.IsNullOrWhiteSpace(dto.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(dto.Content, user.Id);

                // Update user interests based on their own post content
                await _interestService.UpdateUserInterestWeightsAsync(
                    user.Id,
                    createdPost.Id,
                    InteractionType.Create);
            }

            return Ok(createdPost);
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var post = await _postsService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            if (post.UserId != user.Id)
                return Forbid();

            // Process hashtags for removed post
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(post.Content, user.Id);
            
            var postRemoved = await _postsService.DeletePostAsync(postId);
            return Ok(postRemoved);
        }

        [HttpPost("like")]
        public async Task<IActionResult> TogglePostLike([FromBody] PostLikeDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _postsService.TogglePostLikeAsync(dto.PostId, user.Id);
            var post = await _postsService.GetPostByIdAsync(dto.PostId);

            if (result.SendNotification && user.Id != post.UserId)
            {
                await _notificationService.AddNewNotificationAsync(
                    post.UserId, 
                    NotificationType.Like, 
                    user.FullName, 
                    dto.PostId);
            }

            return Ok(post);
        }

        [HttpPost("favorite")]
        public async Task<IActionResult> TogglePostFavorite([FromBody] PostFavoriteDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var result = await _postsService.TogglePostFavoriteAsync(dto.PostId, user.Id);
            var post = await _postsService.GetPostByIdAsync(dto.PostId);

            if (result.SendNotification && user.Id != post.UserId)
            {
                await _notificationService.AddNewNotificationAsync(
                    post.UserId,
                    NotificationType.Favorite,
                    user.FullName,
                    dto.PostId);
            }

            return Ok(post);
        }

        [HttpPost("visibility")]
        public async Task<IActionResult> TogglePostVisibility([FromBody] PostVisibilityDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _postsService.TogglePostVisibilityAsync(dto.PostId, user.Id);
            var post = await _postsService.GetPostByIdAsync(dto.PostId);

            return Ok(new { success = true, isPrivate = post.IsPrivate });
        }

        [HttpPost("report")]
        public async Task<IActionResult> ReportPost([FromBody] PostReportDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _postsService.ReportPostAsync(dto.PostId, user.Id);
            return Ok(new { success = true });
        }

        // Comment methods that use PostService instead of CommentService

        [HttpPost("comment")]
        public async Task<IActionResult> AddPostComment([FromBody] PostCommentDto dto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            var newComment = new Comment()
            {
                UserId = user.Id,
                PostId = dto.PostId,
                Content = dto.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };

            await _postsService.AddPostCommontAsync(newComment);

            var post = await _postsService.GetPostByIdAsync(dto.PostId);
            if (user.Id != post.UserId)
            {
                await _notificationService.AddNewNotificationAsync(
                    post.UserId, 
                    NotificationType.Comment, 
                    user.FullName, 
                    dto.PostId);
            }

            return Ok(post);
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> RemovePostComment(int commentId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
                return Unauthorized();

            await _postsService.DeletePostCommentAsync(commentId);
            return Ok(new { success = true });
        }

        [HttpGet("{postId}/comment-count")]
        public async Task<IActionResult> GetPostCommentCount(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound();

            return Ok(new { count = post.Comment.Count });
        }
    }

    public class CreatePostDto
    {
        public string? Content { get; set; }
        public IFormFile? Image { get; set; }
        public bool IsPrivate { get; set; }
    }

    public class PostLikeDto
    {
        public int PostId { get; set; }
    }

    public class PostFavoriteDto
    {
        public int PostId { get; set; }
    }

    public class PostVisibilityDto
    {
        public int PostId { get; set; }
    }

    public class PostReportDto
    {
        public int PostId { get; set; }
    }

    public class PostCommentDto
    {
        public int PostId { get; set; }
        public string Content { get; set; } = string.Empty;
    }
}