using EtherApp.API.Controllers.Base;
using EtherApp.API.Models;
using EtherApp.Data.Helpers.Constants;
using EtherApp.Data.Helpers.Enums;
using EtherApp.Data.Models;
using EtherApp.Data.Services.Interfaces;
using EtherApp.Shared.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EtherApp.API.Controllers
{
    [Authorize]
    public class PostsController : BaseApiController
    {
        private readonly IPostsService _postsService;
        private readonly IHashtagsService _hashtagsService;
        private readonly IFilesService _filesService;
        private readonly INotificationService _notificationService;
        private readonly IInterestService _interestService;

        public PostsController(
            IPostsService postsService,
            IHashtagsService hashtagsService,
            IFilesService filesService,
            INotificationService notificationService,
            IInterestService interestService)
        {
            _postsService = postsService;
            _hashtagsService = hashtagsService;
            _filesService = filesService;
            _notificationService = notificationService;
            _interestService = interestService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllPosts()
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var allPosts = await _postsService.GetAllPostsAsync(userId.Value);
            return Ok(ApiResponse<List<Post>>.SuccessResponse(allPosts));
        }

        [HttpGet("{postId}")]
        public async Task<IActionResult> GetPostById(int postId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var post = await _postsService.GetPostByIdAsync(postId);
            if (post == null)
                return NotFound(ApiResponse<object>.ErrorResponse("Post not found"));

            return Ok(ApiResponse<Post>.SuccessResponse(post));
        }

        [HttpPost]
        public async Task<IActionResult> CreatePost([FromForm] PostVM post)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            post.Content ??= string.Empty;

            // Handle image upload if present
            string imageUploadPath = null;
            if (post.Image != null)
            {
                imageUploadPath = await _filesService.UploadImageAsync(post.Image, ImageFileType.PostImage);
            }

            if (string.IsNullOrWhiteSpace(post.Content) && string.IsNullOrEmpty(imageUploadPath))
            {
                return BadRequest(ApiResponse<object>.ErrorResponse("Please provide either text content or an image for your post"));
            }

            var newPost = new Post
            {
                Content = post.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
                ImageUrl = imageUploadPath,
                NrOfReports = 0,
                UserId = userId.Value,
                IsPrivate = post.IsPrivate ?? false
            };

            var createdPost = await _postsService.CreatePostAsync(newPost);

            if (!string.IsNullOrWhiteSpace(post.Content))
            {
                await _hashtagsService.ProcessHashtagsForNewPostAsync(post.Content, userId.Value);
                await _interestService.UpdateUserInterestWeightsAsync(
                    userId.Value,
                    createdPost.Id,
                    InteractionType.Create);
            }

            return Ok(ApiResponse<Post>.SuccessResponse(createdPost));
        }

        [HttpPost("like/{postId}")]
        public async Task<IActionResult> TogglePostLike(int postId)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var result = await _postsService.TogglePostLikeAsync(postId, userId.Value);
            var post = await _postsService.GetPostByIdAsync(postId);
            
            // Check if the user has liked the post after the toggle operation
            var isLiked = post.Like != null && post.Like.Any(l => l.UserId == userId.Value);

            if (result.SendNotification && userId != post.UserId)
                await _notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Like, userName, postId);

            return Ok(ApiResponse<object>.SuccessResponse(new { 
                IsLiked = isLiked, // Use calculated value instead of accessing missing property
                LikesCount = post.Like?.Count ?? 0
            }));
        }

        [HttpPost("favorite/{postId}")]
        public async Task<IActionResult> TogglePostFavorite(int postId)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var result = await _postsService.TogglePostFavoriteAsync(postId, userId.Value);
            var post = await _postsService.GetPostByIdAsync(postId);
            
            // Check if the user has favorited the post after the toggle operation
            var isFavorite = post.Favorites != null && post.Favorites.Any(f => f.UserId == userId.Value);

            if (result.SendNotification && userId != post.UserId)
                await _notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Favorite, userName, postId);

            return Ok(ApiResponse<object>.SuccessResponse(new { 
                IsFavorite = isFavorite, // Use calculated value instead of accessing missing property
                FavoritesCount = post.Favorites?.Count ?? 0
            }));
        }

        [HttpPost("visibility/{postId}")]
        public async Task<IActionResult> TogglePostVisibility(int postId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            await _postsService.TogglePostVisibilityAsync(postId, userId.Value);
            var post = await _postsService.GetPostByIdAsync(postId);

            return Ok(ApiResponse<object>.SuccessResponse(new { IsPrivate = post.IsPrivate }));
        }

        [HttpPost("comment")]
        public async Task<IActionResult> AddPostComment([FromBody] PostCommentVM commentVM)
        {
            var userId = GetUserId();
            var userName = GetUserFullName();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var newComment = new Comment()
            {
                UserId = userId.Value,
                PostId = commentVM.PostId,
                Content = commentVM.Content,
                DateCreated = DateTime.Now,
                DateUpdated = DateTime.Now,
            };

            await _postsService.AddPostCommontAsync(newComment);
            var post = await _postsService.GetPostByIdAsync(commentVM.PostId);

            if (userId != post.UserId)
                await _notificationService.AddNewNotificationAsync(post.UserId, NotificationType.Comment, userName, commentVM.PostId);

            return Ok(ApiResponse<object>.SuccessResponse(new { 
                Comment = newComment,
                CommentsCount = post.Comment?.Count ?? 0
            }));
        }

        [HttpPost("report/{postId}")]
        public async Task<IActionResult> ReportPost(int postId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            await _postsService.ReportPostAsync(postId, userId.Value);
            return Ok(ApiResponse<object>.SuccessResponse("Post reported successfully"));
        }

        [HttpDelete("comment/{commentId}")]
        public async Task<IActionResult> DeleteComment(int commentId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            await _postsService.DeletePostCommentAsync(commentId);
            return Ok(ApiResponse<object>.SuccessResponse("Comment deleted successfully"));
        }

        [HttpDelete("{postId}")]
        public async Task<IActionResult> DeletePost(int postId)
        {
            var userId = GetUserId();
            if (!userId.HasValue)
                return Unauthorized(ApiResponse<object>.ErrorResponse("User not authenticated"));

            var postRemoved = await _postsService.DeletePostAsync(postId);
            await _hashtagsService.ProcessHashtagsForRemovedPostAsync(postRemoved.Content, postRemoved.UserId);

            return Ok(ApiResponse<object>.SuccessResponse("Post deleted successfully"));
        }

        [HttpGet("comments/{postId}")]
        public async Task<IActionResult> GetCommentCount(int postId)
        {
            var post = await _postsService.GetPostByIdAsync(postId);
            return Ok(ApiResponse<int>.SuccessResponse(post.Comment?.Count ?? 0));
        }
    }
}