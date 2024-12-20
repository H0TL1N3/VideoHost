using Microsoft.AspNetCore.Identity;

namespace VideoHost.Server.Models
{
    public class User : IdentityUser<int>
    {
        public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

        public required string DisplayName { get; set; }

        public ICollection<Video> Videos { get; set; } = new List<Video>();

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();

        public ICollection<Subscription> Subscribers { get; set; } = new List<Subscription>();
    }
}
