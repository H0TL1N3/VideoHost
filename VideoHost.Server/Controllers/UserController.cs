using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

using VideoHost.Server.Data;
using VideoHost.Server.Models;

namespace VideoHost.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<IdentityRole<int>> _roleManager;
        private readonly ApplicationDbContext _dbContext;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;

        public UserController(UserManager<User> userManager, RoleManager<IdentityRole<int>> roleManager, SignInManager<User> signInManager, IConfiguration configuration, ApplicationDbContext dbContext)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _dbContext = dbContext;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get([FromQuery] string id)
        {
            if (string.IsNullOrEmpty(id))
                return BadRequest(new { message = "Id is required." });

            var user = await _userManager.FindByIdAsync(id);

            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            return Ok(new
            {
                id = user.Id,
                email = user.Email,
                displayName = user.DisplayName,
                registrationDate = user.RegistrationDate
            });
        }

        [HttpGet("get-many")]
        public async Task<IActionResult> GetMany(
            string? searchTerm = null,
            string? orderBy = null,
            int skip = 0,
            int take = 8)
        {
            var query = _userManager.Users;

            // Filter by search term if it's provided
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.DisplayName.Contains(searchTerm));
            }

            // Include subscriber count using a subquery
            var usersWithSubscriberCount = query
                .Select(user => new
                {
                    user.Id,
                    user.DisplayName,
                    user.Email,
                    user.RegistrationDate,
                    SubscriberCount = _dbContext.Subscriptions.Count(s => s.SubscribedToId == user.Id)
                });

            usersWithSubscriberCount = orderBy?.ToLower() switch
            {
                "subscribers" => usersWithSubscriberCount.OrderByDescending(u => u.SubscriberCount),
                _ => usersWithSubscriberCount.OrderByDescending(u => u.RegistrationDate)
            };

            var users = await usersWithSubscriberCount.Skip(skip).Take(take).ToListAsync();

            return Ok(users);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var user = new User
            {
                DisplayName = request.Username,
                UserName = request.Email,
                Email = request.Email,
                RegistrationDate = DateTime.UtcNow,
                EmailConfirmed = true
            };

            var result = await _userManager.CreateAsync(user, request.Password);

            if (!result.Succeeded)
                return BadRequest(result.Errors);

            await _userManager.AddToRoleAsync(user, "User");

            return Ok(new { message = "Registration successful! You may login now." });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(new { message = "Your request data is invalid.", errors = ModelState });

            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            var result = await _signInManager.PasswordSignInAsync(request.Email, request.Password, true, false);
            if (!result.Succeeded)
                return Unauthorized(new { message = "The password is incorrect." });

            var roles = await _userManager.GetRolesAsync(user);
            var role = roles.FirstOrDefault();

            return Ok(new
            {
                token = GenerateJwtToken(user),
                user = new
                {
                    user.Id,
                    user.DisplayName,
                    user.Email,
                    user.RegistrationDate,
                    role
                },
                message = "Login successful!"
            });
        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return Ok(new { message = "You are now logged out." });
        }

        [Authorize]
        [HttpPut("update-display-name")]
        public async Task<IActionResult> UpdateDisplayName([FromBody] UserUpdateRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound(new { message = "User not found." });

            user.DisplayName = request.UserName;

            await _userManager.UpdateAsync(user);

            return Ok(new { message = "The user info has been updated successfully!" });
        }

        [Authorize]
        [HttpPut("update-sensitive-data")]
        public async Task<IActionResult> UpdateSensitiveData([FromBody] UserUpdateSensitiveDataRequest request)
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound(new { message = "User not found." });

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.OldPassword);
            if (!isPasswordValid)
                return BadRequest(new { message = "Old password is incorrect." });

            // Update email and username if provided
            if (!string.IsNullOrWhiteSpace(request.NewEmail))
            {
                var emailResult = await _userManager.SetEmailAsync(user, request.NewEmail);
                if (!emailResult.Succeeded)
                    return BadRequest(new { message = "Failed to update email.", errors = emailResult.Errors });

                var usernameResult = await _userManager.SetUserNameAsync(user, request.NewEmail);
                if (!usernameResult.Succeeded)
                    return BadRequest(new { message = "Failed to update username.", errors = usernameResult.Errors });
            }

            // Update password if provided
            if (!string.IsNullOrWhiteSpace(request.NewPassword))
            {
                var passwordResult = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);
                if (!passwordResult.Succeeded)
                    return BadRequest(new { message = "Failed to update password.", errors = passwordResult.Errors });
            }

            // Update the user in the database
            await _userManager.UpdateAsync(user);

            // Sign out the user to update user store
            await _signInManager.SignOutAsync();

            return Ok(new { message = "Sensitive data updated successfully! Please, login again to see changes." });
        }

        // Please note that the DeleteUserVideos method of the VideoController should be ran first.
        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete()
        {
            var user = await _userManager.GetUserAsync(User);

            if (user == null)
                return NotFound(new { message = "This user does not exist." });

            // Sign out the user first
            await _signInManager.SignOutAsync();         

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

            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "The user has been deleted." });
        }

        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddDays(7),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }

    public class UserUpdateRequest
    {
        public required string UserName { get; set; }
    }

    public class UserUpdateSensitiveDataRequest
    {
        public required string OldPassword { get; set; }
        public string? NewEmail { get; set; }
        public string? NewPassword { get; set; }
    }

public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    public class RegisterRequest
    {
        public required string Username { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
    }
}
