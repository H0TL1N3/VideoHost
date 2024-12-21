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

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int videoId, [FromQuery] int skip = 0, [FromQuery] int take = 10)
        {
            var comments = await _dbContext.Comments
                .Where(c => c.VideoId == videoId)
                .OrderBy(c => c.CreationDate)
                .Skip(skip)
                .Take(take)
                .Select(c => new
                {
                    c.Id,
                    c.Content,
                    c.CreationDate,
                    UserName = c.User.DisplayName,
                    c.UserId
                })
                .ToListAsync();

            return Ok(comments);
        }

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
