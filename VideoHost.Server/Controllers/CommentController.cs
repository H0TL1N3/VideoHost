using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VideoHost.Server.Data;
using VideoHost.Server.Models;

namespace VideoHost.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CommentController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public CommentController(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }  

        [HttpGet("get")]
        public async Task<IActionResult> Get(
            int? videoId = null,
            int? userId = null,
            int? subscriberId = null,
            int skip = 0,
            int take = 10)
        {
            IQueryable<Comment> query = _dbContext.Comments
                .Include(c => c.User)
                .Include(c => c.Video);

            // Filter by video if videoId is provided
            if (videoId.HasValue)
                query = query.Where(c => c.VideoId == videoId.Value);

            // Filter by user if userId is provided
            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value);

            // Filter by subscriptions if subscriberId is provided
            if (subscriberId.HasValue)
            {
                var subscribedUserIds = await _dbContext.Subscriptions
                    .Where(s => s.SubscriberId == subscriberId.Value)
                    .Select(s => s.SubscribedToId)
                    .ToListAsync();

                query = query.Where(c => subscribedUserIds.Contains(c.UserId));
            }

            // Sort comments by creation date and paginate
            var comments = await query
                .OrderBy(c => c.CreationDate)
                .Skip(skip)
                .Take(take)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreationDate,
                    UserName = c.User.DisplayName,
                    c.UserId,
                    Video = new
                        {
                            c.Video.Id,
                            c.Video.Name
                        }
                })
                .ToListAsync();

            return Ok(comments);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] CommentAddRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Content))
                return BadRequest(new { message = "Content cannot be empty." });

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found." });

            var video = await _dbContext.Videos.FindAsync(request.VideoId);
            if (video == null)
                return NotFound(new { message = "Video not found." });

            var comment = new Comment
            {
                Content = request.Content,
                CreationDate = DateTime.UtcNow,
                UserId = request.UserId,
                User = user,
                VideoId = request.VideoId,
                Video = video
            };

            _dbContext.Comments.Add(comment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Comment added successfully!" });
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] CommentUpdateRequest request)
        {
            var comment = await _dbContext.Comments.FindAsync(request.Id);

            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            comment.Content = request.Content;
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Comment updated successfully!" });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var comment = await _dbContext.Comments.FindAsync(id);

            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            _dbContext.Comments.Remove(comment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Comment deleted successfully." });
        }
    }

    public class CommentAddRequest
    {
        public required string Content { get; set; }
        public required int VideoId { get; set; }
        public required int UserId { get; set; }
    }

    public class CommentUpdateRequest
    {
        public required int Id { get; set; }
        public required string Content { get; set; }
    }
}
