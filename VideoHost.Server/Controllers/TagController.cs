using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VideoHost.Server.Data;
using VideoHost.Server.Models;

namespace VideoHost.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        public TagController(ApplicationDbContext context)
        {
            _dbContext = context;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get()
        {
            var tags = await _dbContext.Tags.ToListAsync();

            if (tags == null || tags.Count == 0)
                return NotFound(new { message = "No tags exist in the database." });

            return Ok(tags);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] TagAddRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { message = "Tag name is required." });

            if (await _dbContext.Tags.AnyAsync(t => t.Name == request.Name))
                return Conflict(new { message = "Tag already exists." });

            var tag = new Tag
            {
                Name = request.Name,
                Description = request.Description
            };

            _dbContext.Tags.Add(tag);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Tag created successfully!" });
        }

        [Authorize]
        [HttpPost("attach")]
        public async Task<IActionResult> Attach([FromBody] TagAttachRequest request)
        {
            var video = await _dbContext.Videos.Include(v => v.VideoTags).FirstOrDefaultAsync(v => v.Id == request.videoId);
            if (video == null)
                return NotFound(new { message = "Video not found." });

            _dbContext.VideoTags.RemoveRange(video.VideoTags!);

            if (request.TagIds != null && request.TagIds.Any())
            {
                var tags = await _dbContext.Tags.Where(t => request.TagIds.Contains(t.Id)).ToListAsync();

                foreach (var tag in tags)
                {
                    video.VideoTags!.Add(new VideoTag
                    {
                        VideoId = video.Id,
                        Video = video,
                        TagId = tag.Id,
                        Tag = tag
                    });
                }
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = "Tags attached successfully." });
        }
    }

    public class TagAddRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
    }

    public class TagAttachRequest
    {
        public required int videoId { get; set; }
        public IEnumerable<int> TagIds { get; set; } = new List<int>();
    }
}
