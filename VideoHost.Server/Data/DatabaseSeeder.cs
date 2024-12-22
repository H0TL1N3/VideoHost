using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

using VideoHost.Server.Models;

namespace VideoHost.Server.Data
{
    public static class DatabaseSeeder
    {
        public static async Task Seed(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole<int>>>();
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            // Seed Roles
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole<int> { Name = role });
                }
            }

            // Seed Users
            // UserName and Email are the same because email is used as a unique identifier for the user:
            // Such is the requirement of Microsoft's Identity: https://stackoverflow.com/a/35194385
            if (await userManager.FindByEmailAsync("admin@example.com") == null)
            {
                var admin = new User
                {
                    UserName = "admin@example.com",
                    Email = "admin@example.com",
                    DisplayName = "Administrator",
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(admin, "Testing1!");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }

            var regularUsers = new[]
            {
                new { UserName = "user1@example.com", Email = "user1@example.com", DisplayName = "User1" },
                new { UserName = "user2@example.com", Email = "user2@example.com", DisplayName = "User2" }
            };

            foreach (var userData in regularUsers)
            {
                if (await userManager.FindByEmailAsync(userData.Email) == null)
                {
                    var user = new User
                    {
                        UserName = userData.UserName,
                        Email = userData.Email,
                        DisplayName = userData.DisplayName,
                        EmailConfirmed = true
                    };

                    var result = await userManager.CreateAsync(user, "Testing1!");
                    if (result.Succeeded)
                    {
                        await userManager.AddToRoleAsync(user, "User");
                    }
                }
            }

            // Seed Tags
            var tags = new[]
            {
                "Music", "Gaming", "Vlog", "Tech", "History"
            };

            foreach (var tagName in tags)
            {
                if (!await dbContext.Tags.AnyAsync(t => t.Name == tagName))
                {
                    dbContext.Tags.Add(new Tag { Name = tagName });
                }
            }

            await dbContext.SaveChangesAsync();
        }
    }
}
