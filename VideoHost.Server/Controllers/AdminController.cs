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
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;

        public AdminController(ApplicationDbContext dbContext, UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // Generic method to retrieve a single entity
        [HttpGet("get-entity")]
        public async Task<IActionResult> GetEntity(string entityType, int id)
        {
            switch (entityType.ToLower())
            {
                // Tags
                case "tags":
                    var tag = await _dbContext.Tags
                        .FirstOrDefaultAsync(c => c.Id == id);

                    if (tag == null)
                        return NotFound(new { message = "Entity does not exist." });

                    return Ok(new
                    {
                        tag.Id,
                        tag.Name
                    });

                // Videos
                case "videos":
                    var video = await _dbContext.Videos
                    .Include(v => v.User)
                    .Include(v => v.VideoTags)!
                        .ThenInclude(vt => vt.Tag)
                    .FirstOrDefaultAsync(v => v.Id == id);

                    if (video == null)
                        return NotFound(new { message = "Entity does not exist." });

                    return Ok(new
                    {
                        video.Id,
                        video.Name,
                        video.Description,
                        userId = video.User.Id,
                        Users = _dbContext.Users.Select(u => new
                        {
                            u.Id,
                            u.DisplayName
                        }),
                        Tags = video.VideoTags!.Select(vt => new
                        {
                            vt.Tag.Id,
                            vt.Tag.Name
                        })
                    });

                // Comments
                case "comments":
                    var comment = await _dbContext.Comments
                    .Include(c => c.User)
                    .Include(c => c.Video)
                    .FirstOrDefaultAsync(c => c.Id == id);

                    if (comment == null)
                        return NotFound(new { message = "Entity does not exist." });

                    return Ok(new
                    {
                        comment.Id,
                        comment.Content,
                        User = new
                        {
                            comment.User.Id,
                            comment.User.DisplayName,     
                        },
                        Video = new
                        {
                            comment.Video.Id,
                            comment.Video.Name
                        },
                        Users = _dbContext.Users.Select(u => new
                        {
                            u.Id,
                            u.DisplayName
                        }),
                        Videos = _dbContext.Videos.Select(v => new
                        {
                            v.Id,
                            v.Name
                        })
                    });

                // Users
                case "users":
                    var user = await _dbContext.Users
                        .FirstOrDefaultAsync(u => u.Id == id);

                    if (user == null)
                        return NotFound(new { message = "Entity does not exist." });

                    var userRoles = await _userManager.GetRolesAsync(user);

                    var allRoles = await _roleManager.Roles
                        .Select(r => r.Name)
                        .ToListAsync();

                    return Ok(new
                    {
                        user.Id,
                        user.DisplayName,
                        user.Email,
                        Role = userRoles.FirstOrDefault(),
                        Roles = allRoles
                    });

                // Subscriptions
                case "subscriptions":
                    var subscription = await _dbContext.Subscriptions
                    .Include(s => s.Subscriber)
                    .Include(s => s.SubscribedTo)
                    .FirstOrDefaultAsync(s => s.Id == id);

                    if (subscription == null)
                        return NotFound(new { message = "Entity does not exist." });

                    return Ok(new
                    {
                        subscription.Id,
                        Subscriber = new
                        {
                            subscription.Subscriber.Id,
                            subscription.Subscriber.DisplayName,
                        },
                        SubscribedTo = new
                        {
                            subscription.SubscribedTo.Id,
                            subscription.SubscribedTo.DisplayName,
                        },
                        Users = _dbContext.Users.Select(u => new
                        {
                            u.Id,
                            u.DisplayName
                        })
                    });              

                default:
                    return BadRequest("Invalid entity type.");
            }
        }

        // Generic method to retrieve entities as a paginated list
        [HttpGet("get-entities")]
        public async Task<IActionResult> GetEntities(string entityType, int skip = 0, int take = 10)
        {
            switch (entityType.ToLower())
            {
                // Tags
                case "tags":
                    var tags = await _dbContext.Tags
                        .Skip(skip)
                        .Take(take)
                        .Select(t => new
                        {
                            t.Id,
                            t.Name
                        })
                        .ToListAsync();

                    return Ok(tags);

                // Videos
                case "videos":
                    var videos = await _dbContext.Videos
                        .Include(v => v.User)
                        .Skip(skip)
                        .Take(take)
                        .Select(v => new
                        {
                            v.Id,
                            v.Name,
                            v.ThumbnailPath,
                            v.UploadDate,
                            v.Description,
                            v.ViewCount,
                            User = new
                            {
                                v.User.Id,
                                v.User.DisplayName
                            }
                        }).ToListAsync();

                    return Ok(videos);

                // Comments
                case "comments":
                    var comments = await _dbContext.Comments
                        .Include(c => c.User)
                        .Include(c => c.Video)
                        .Skip(skip)
                        .Take(take)
                        .Select(c => new
                        {
                            c.Id,
                            c.Content,
                            c.CreationDate,
                            User = new
                            {
                                c.User.DisplayName,
                                c.User.Id
                            },
                            Video = new
                            {
                                c.Video.Id,
                                c.Video.Name
                            }
                        })
                        .ToListAsync();

                    return Ok(comments);

                // Users
                case "users":
                    var users = await _dbContext.Users
                        .Skip(skip)
                        .Take(take)
                        .Select(u => new
                        {
                            u.Id,
                            u.DisplayName,
                            u.Email,
                            u.RegistrationDate,
                        }).ToListAsync();

                    return Ok(users);

                // Subscriptions
                case "subscriptions":
                    var subscriptions = await _dbContext.Subscriptions
                        .Skip(skip)
                        .Take(take)
                        .Select(s => new
                        {
                            s.Id,
                            s.SubscriptionDate,
                            Subscriber = new
                            {
                                s.Subscriber.Id,
                                s.Subscriber.DisplayName,
                                s.Subscriber.Email
                            },
                            SubscribedTo = new
                            {
                                s.SubscribedTo.Id,
                                s.SubscribedTo.DisplayName,
                                s.SubscribedTo.Email
                            }
                        }).ToListAsync();

                    return Ok(subscriptions);

                default:
                    return BadRequest("Invalid entity type.");
            }
        }      

        [HttpPut("update-tag")]
        public async Task<IActionResult> UpdateTag([FromBody] AdminTagUpdateRequest request)
        {
            var tag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Id == request.Id);
            if (tag == null)
                return NotFound(new { message = "This tag does not exist." });

            var sameTag = await _dbContext.Tags.FirstOrDefaultAsync(t => t.Name == request.Name);
            if (sameTag != null && sameTag.Name == request.Name && sameTag.Id != request.Id)
                return Conflict(new { message = "Tag already exists." });           

            tag.Name = request.Name;

            _dbContext.Tags.Update(tag);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "The Tag has been updated successfully!" });
        }

        [HttpPut("update-video")]
        public async Task<IActionResult> UpdateVideo([FromBody] AdminVideoUpdateRequest request)
        {
            var video = await _dbContext.Videos
                .Include(v => v.VideoTags)
                .FirstOrDefaultAsync(v => v.Id == request.Id);
            if (video == null)
                return NotFound(new { message = "This video does not exist." });

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            // Update base info
            video.Name = request.Name;
            video.Description = request.Description;
            video.UserId = user.Id;
            video.User = user;

            // Update video tags
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

            _dbContext.Videos.Update(video);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "The Video has been updated successfully!" });
        }

        [HttpPut("update-comment")]
        public async Task<IActionResult> UpdateComment([FromBody] AdminCommentUpdateRequest request)
        {
            var comment = await _dbContext.Comments.FirstOrDefaultAsync(s => s.Id == request.Id);
            if (comment == null)
                return NotFound(new { message = "Comment not found." });

            var video = await _dbContext.Videos.FindAsync(request.VideoId);
            if (video == null)
                return NotFound(new { message = "Video not found." });

            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                return NotFound(new { message = "User not found." });

            comment.Content = request.Content;
            comment.VideoId = video.Id;
            comment.Video = video;
            comment.UserId = user.Id;
            comment.User = user;              

            _dbContext.Comments.Update(comment);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Comment data updated successfully!" });
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] AdminUserUpdateRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString());
            if (user == null)
                return NotFound(new { message = "User not found." });

            // Update displayName if provided and if differs
            if (!string.IsNullOrWhiteSpace(request.DisplayName) && request.DisplayName != user.DisplayName)
                user.DisplayName = request.DisplayName;

            // Update email and username if provided and if differs
            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var usernameResult = await _userManager.SetUserNameAsync(user, request.Email);
                if (!usernameResult.Succeeded)
                    return BadRequest(new { message = "Failed to update username.", errors = usernameResult.Errors });

                var emailResult = await _userManager.SetEmailAsync(user, request.Email);
                if (!emailResult.Succeeded)
                    return BadRequest(new { message = "Failed to update email.", errors = emailResult.Errors });               
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                var code = await _userManager.GeneratePasswordResetTokenAsync(user);
                var passwordResult = await _userManager.ResetPasswordAsync(user, code, request.NewPassword);
                if (!passwordResult.Succeeded)
                    return BadRequest(new { message = "Failed to update password.", errors = passwordResult.Errors });
            }

            // Update role if the new one differs from the old one
            var currentRoles = await _userManager.GetRolesAsync(user);

            if (!!string.IsNullOrWhiteSpace(request.Role) && currentRoles.Contains(request.Role))
            {
                var roleToRemove = currentRoles.FirstOrDefault(r => r != request.Role);
                if (roleToRemove != null)
                {
                    var removeResult = await _userManager.RemoveFromRoleAsync(user, roleToRemove);
                    if (!removeResult.Succeeded)
                        return BadRequest(new { message = "Failed to remove role.", errors = removeResult.Errors });
                }

                var addResult = await _userManager.AddToRoleAsync(user, request.Role);
                if (!addResult.Succeeded)
                    return BadRequest(new { message = "Failed to add role.", errors = addResult.Errors });
            }      

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "User data updated successfully!" });
        }

        [HttpPut("update-subscription")]
        public async Task<IActionResult> UpdateSubscription([FromBody] AdminSubscriptionUpdateRequest request)
        {
            var subscription = await _dbContext.Subscriptions.FirstOrDefaultAsync(s => s.Id == request.Id);
            if (subscription == null)
                return NotFound(new { message = "Subscription not found." });

            var subscriber = await _userManager.FindByIdAsync(request.SubscriberId.ToString());
            var subscribedTo = await _userManager.FindByIdAsync(request.SubscribedToId.ToString());

            if (subscriber == null || subscribedTo == null)
                return NotFound(new { message = "Either the subscriber or the subscription target does not exist." });

            if (subscriber == subscribedTo)
                return Conflict(new { message = "The subscriber and the subscription target cannot be the same user." });

            subscription.SubscriberId = subscriber.Id;
            subscription.Subscriber = subscriber;
            subscription.SubscribedToId = subscribedTo.Id;
            subscription.SubscribedTo = subscribedTo;

            _dbContext.Subscriptions.Update(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "Subscription data updated successfully!" });
        }

        // Generic method to delete an entity
        [HttpDelete("delete-entity")]
        public async Task<IActionResult> DeleteEntity(string entityType, int id)
        {
            switch (entityType.ToLower())
            {
                // Tag
                case "tag":
                    var tag = await _dbContext.Tags.FindAsync(id);

                    if (tag == null)
                        return NotFound();

                    _dbContext.Tags.Remove(tag);

                    break;

                // Video
                case "video":
                    var video = await _dbContext.Videos.FindAsync(id);

                    if (video == null)
                        return NotFound();

                    DeleteVideoFile(video);

                    _dbContext.Videos.Remove(video);

                    break;

                // Comment
                case "comment":
                    var comment = await _dbContext.Comments.FindAsync(id);

                    if (comment == null)
                        return NotFound();

                    _dbContext.Comments.Remove(comment);

                    break;

                // Users
                case "user":
                    var user = await _userManager.FindByIdAsync(id.ToString());

                    if (user == null)
                        return NotFound(new { message = "This user does not exist." });

                    // Delete related videos
                    var videos = await _dbContext.Videos
                        .Where(v => v.UserId == id)
                        .ToListAsync();

                    if (videos.Count > 0)
                    {
                        foreach (Video videoFile in videos)
                            DeleteVideoFile(videoFile);

                        _dbContext.Videos.RemoveRange(videos);
                    }                    

                    // Delete related subscriptions
                    var subscriptions = await _dbContext.Subscriptions
                        .Where(s => s.SubscribedToId == user.Id)
                        .ToListAsync();

                    var subscribedTo = await _dbContext.Subscriptions
                        .Where(s => s.SubscriberId == user.Id)
                        .ToListAsync();

                    if (subscriptions.Count > 0)
                        _dbContext.Subscriptions.RemoveRange(subscriptions);

                    if (subscribedTo.Count > 0)
                        _dbContext.Subscriptions.RemoveRange(subscribedTo);

                    // Delete related comments
                    var comments = await _dbContext.Comments
                        .Where(c => c.UserId == user.Id)
                        .ToListAsync();

                    if (comments.Count > 0)
                        _dbContext.Comments.RemoveRange(comments);

                    // Delete the user 
                    await _userManager.DeleteAsync(user);

                    break;

                // Subscription
                case "subscription":
                    var subscription = await _dbContext.Subscriptions.FindAsync(id);

                    if (subscription == null)
                        return NotFound();

                    _dbContext.Subscriptions.Remove(subscription);

                    break;               

                default:
                    return BadRequest("Invalid entity type.");
            }

            await _dbContext.SaveChangesAsync();
            return Ok(new { message = $"{entityType} with ID {id} has been deleted." });
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

    public class AdminTagUpdateRequest
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
    }

    public class AdminVideoUpdateRequest
    {
        public required int Id { get; set; }
        public required string Name { get; set; }
        public required int UserId { get; set; }
        public string? Description { get; set; }
        public IEnumerable<int>? TagIds { get; set; } = new List<int>();
    }

    public class AdminCommentUpdateRequest
    {
        public required int Id { get; set; }
        public required int UserId { get; set; }
        public required int VideoId { get; set; }
        public required string Content { get; set; }
    }

    public class AdminUserUpdateRequest
    {
        public required int Id { get; set; }
        public required string Role { get; set; }
        public string? DisplayName { get; set; }
        public string? Email { get; set; }
        public string? NewPassword { get; set; }
    }

    public class AdminSubscriptionUpdateRequest
    {
        public required int Id { get; set; }
        public required int SubscriberId { get; set; }
        public required int SubscribedToId { get; set; }
    }
}
