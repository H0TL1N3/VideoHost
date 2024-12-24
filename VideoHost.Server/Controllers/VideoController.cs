using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using Xabe.FFmpeg;

using VideoHost.Server.Data;
using VideoHost.Server.Models;

namespace VideoHost.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class VideoController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly IWebHostEnvironment _environment;

        public VideoController(UserManager<User> userManager, IWebHostEnvironment environment, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _dbContext = dbContext;
            _environment = environment;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] int id)
        {
            var video = await _dbContext.Videos
                .Include(v => v.User)
                .Include(v => v.VideoTags)!
                    .ThenInclude(vt => vt.Tag)
                .FirstOrDefaultAsync(v => v.Id == id);

            if (video == null)
                return NotFound(new { message = "This video does not exist." });

            return Ok(new
            {
                video.Id,
                video.Name,
                video.UploadPath,
                video.UploadDate,
                video.Description,
                video.ViewCount,
                User = new
                {
                    video.User.Id,
                    video.User.DisplayName
                },
                Tags = video.VideoTags!.Select(vt => new { 
                    vt.Tag.Id,
                    vt.Tag.Name
                })
            });
        }

        [HttpGet("get-many")]
        public async Task<IActionResult> GetMany(
            string? searchTerm = null,
            string? tagIds = null,
            int? userId = null,
            int? subscriberId = null,
            string? orderBy = null,
            int skip = 0,
            int take = 8)
        {
            IQueryable<Video> query = _dbContext.Videos
                .Include(v => v.User)
                .Include(v => v.VideoTags)!
                .ThenInclude(vt => vt.Tag);

            // Filter by user if userId is provided
            if (userId.HasValue)
                query = query.Where(v => v.UserId == userId);

            // Filter by user subscriptions if subscriberId is provided
            if (subscriberId.HasValue)
            {
                var subscribedUserIds = await _dbContext.Subscriptions
                    .Where(us => us.SubscriberId == subscriberId.Value)
                    .Select(us => us.SubscribedToId)
                    .ToListAsync();

                query = query.Where(v => subscribedUserIds.Contains(v.UserId));
            }

            // Filter by tags if tagIds are provided
            IEnumerable<int>? tagIdList = null;
            if (!string.IsNullOrEmpty(tagIds))
            {
                tagIdList = System.Text.Json.JsonSerializer.Deserialize<List<int>>(tagIds);
                query = query.Where(v =>
                    tagIdList!.All(tagId => v.VideoTags!.Any(vt => vt.TagId == tagId))
                );
            }                

            // Filter by search term if it's provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(v => v.Name.Contains(searchTerm));

            query = orderBy switch
            {
                "views" => query.OrderByDescending(v => v.ViewCount),
                _ => query.OrderByDescending(v => v.UploadDate), // Default ordering by UploadDate
            };

            var videos = await query.Skip(skip).Take(take).Select(v => new
            {
                v.Id,
                v.Name,
                v.ThumbnailPath,
                v.UploadDate,
                v.Description,
                v.ViewCount,
                User = new { 
                    v.User.Id,
                    v.User.DisplayName
                }
            }).ToListAsync();

            return Ok(videos);
        }

        [Authorize]
        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] VideoUploadRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            if (request.VideoFile == null || request.VideoFile.Length == 0)
                return BadRequest(new { message = "No video file provided." });

            if (request.VideoFile.Length > 500 * 1024 * 1024)
                return BadRequest(new { message = "Video file exceeds the maximum allowed size of 500MB." });

            if (request.VideoFile.ContentType != "video/mp4" && Path.GetExtension(request.VideoFile.FileName)?.ToLower() != ".mp4")
                return BadRequest(new { message = "Only MP4 video files are allowed." });

            try
            {
                // Save video to user-specific folder
                string uploadDir = Path.Combine("uploads", user.Id.ToString());
                Directory.CreateDirectory(uploadDir);
                string videoFileName = $"{Path.GetRandomFileName()}.mp4";
                string videoPath = Path.Combine(uploadDir, videoFileName);
                using (var stream = new FileStream(videoPath, FileMode.Create))
                {
                    await request.VideoFile.CopyToAsync(stream);
                }

                // Generate thumbnail using FFmpeg
                string thumbnailFileName = Path.ChangeExtension(videoFileName, ".jpg");
                string thumbnailPath = Path.Combine(uploadDir, thumbnailFileName);
                var videoInfo = await FFmpeg.GetMediaInfo(videoPath);
                TimeSpan middleTimestamp = videoInfo.Duration / 2;
                var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(videoPath, thumbnailPath, middleTimestamp);
                await conversion.Start();

                var video = new Video
                {
                    Name = request.Name,
                    UploadPath = videoPath,
                    ThumbnailPath = thumbnailPath,
                    Description = request.Description ?? String.Empty,
                    UserId = user.Id,
                    User = user,
                    UploadDate = DateTime.UtcNow
                };

                _dbContext.Videos.Add(video);
                await _dbContext.SaveChangesAsync();

                return Ok(new { 
                    message = "Video uploaded successfully!",
                    videoId = video.Id
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while uploading the video.", error = ex.Message });
            }
        }

        [HttpPost("increment")]
        public async Task<IActionResult> Increment([FromBody] VideoIncrementRequest request)
        {
            var video = await _dbContext.Videos.FirstOrDefaultAsync(v => v.Id == request.Id);

            if (video == null)
                return NotFound(new { message = "This video does not exist." });

            video.ViewCount++;

            await _dbContext.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] VideoUpdateRequest request)
        {
            var video = await _dbContext.Videos.FindAsync(request.Id);

            if (video == null)
                return NotFound(new { message = "Video not found." });

            video.Name = request.Name;
            if (!String.IsNullOrWhiteSpace(request.Description))
                video.Description = request.Description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "The video has been updated successfully!" });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            var video = await _dbContext.Videos.FindAsync(id);

            if (video == null)
                return NotFound(new { message = "Video not found." });

            DeleteVideoFile(video);

            _dbContext.Videos.Remove(video);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Video deleted successfully." });
        }

        [Authorize]
        [HttpDelete("delete-user-videos")]
        public async Task<IActionResult> DeleteUserVideos()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            var videos = _dbContext.Videos.Where(v => v.UserId == user.Id);

            foreach (Video video in videos)
                DeleteVideoFile(video);

            _dbContext.Videos.RemoveRange(videos);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "All videos of the user with the Id " + user.Id + " have been deleted." });
        }

        private void DeleteVideoFile(Video video)
        {
            var videoPath = video.UploadPath;
            var thumbnailPath = video.ThumbnailPath;

            try
            {
                if (System.IO.File.Exists(videoPath))
                    System.IO.File.Delete(videoPath);

                if (System.IO.File.Exists(thumbnailPath))
                    System.IO.File.Delete(thumbnailPath);

                return;

            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("An error occurred while deleting the video.", ex);
            }
        }
    }

    public class VideoIncrementRequest
    {
        public required int Id { get; set; }
    }

    public class VideoUploadRequest
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public required IFormFile VideoFile { get; set; }
    }

    public class VideoUpdateRequest
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
