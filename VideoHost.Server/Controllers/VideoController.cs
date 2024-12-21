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
        public async Task<IActionResult> Get([FromQuery] string videoId)
        {
            if (string.IsNullOrEmpty(videoId))
                return BadRequest(new { message = "A video ID is required." });

            var video = await _dbContext.Videos
                .Include(v => v.User) 
                .FirstOrDefaultAsync(v => v.Id == Int32.Parse(videoId));

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
                }
            });
        }

        [HttpPost("upload")]
        public async Task<IActionResult> Upload([FromForm] VideoUploadRequest request)
        {
            if (request.VideoFile == null || request.VideoFile.Length == 0)
                return BadRequest(new { message = "No video file provided." });

            if (request.VideoFile.Length > 500 * 1024 * 1024)
                return BadRequest(new { message = "Video file exceeds the maximum allowed size of 500MB." });

            if (request.VideoFile.ContentType != "video/mp4" && Path.GetExtension(request.VideoFile.FileName)?.ToLower() != ".mp4")
                return BadRequest(new { message = "Only MP4 video files are allowed." });

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found." });   

            try
            {
                // Save video to user-specific folder
                string uploadDir = Path.Combine("uploads", request.UserId.ToString());
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
                    UserId = request.UserId,
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

        [HttpPut("update")]
        public async Task<IActionResult> Update([FromBody] VideoUpdateRequest request)
        {
            var video = await _dbContext.Videos.FindAsync(request.Id);

            if (video == null)
                return NotFound(new { message = "Video not found." });

            video.Name = request.Name;
            video.Description = request.Description;

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "The video has been updated successfully!" });
        }

        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var video = await _dbContext.Videos.FindAsync(id);

                if (video == null)
                    return NotFound(new { message = "Video not found." });

                var videoPath = video.UploadPath;
                var thumbnailPath = video.ThumbnailPath;

                if (System.IO.File.Exists(videoPath))
                    System.IO.File.Delete(videoPath);

                if (System.IO.File.Exists(thumbnailPath))
                    System.IO.File.Delete(thumbnailPath);

                _dbContext.Videos.Remove(video);

                await _dbContext.SaveChangesAsync();

                return Ok(new { message = "Video deleted successfully." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while deleting the video.", error = ex.Message });
            }
        }

    }

    public class VideoUploadRequest
    {
        public required string Name { get; set; }

        public string? Description { get; set; }

        public required int UserId { get; set; }

        public required IFormFile VideoFile { get; set; }
    }

    public class VideoUpdateRequest
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public string? Description { get; set; }
    }
}
