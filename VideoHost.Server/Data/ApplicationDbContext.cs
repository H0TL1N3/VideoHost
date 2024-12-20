using Microsoft.EntityFrameworkCore;

using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace VideoHost.Server.Data
{
    using Models;

    public class ApplicationDbContext : IdentityDbContext<User, IdentityRole<int>, int>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Comment> Comments { get; set; }

        public DbSet<Subscription> Subscriptions { get; set; }

        public DbSet<Tag> Tags { get; set; }

        public DbSet<Video> Videos { get; set; }

        public DbSet<VideoTag> VideoTags { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Run base method to ensure Identity configuration for User functionality
            base.OnModelCreating(modelBuilder);

            // Mapping subscribers both ways - cascade delete is not possible due to EF Core limitations -
            // deletion of subscriptions will be handled when a user is deleted
            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.Subscriber)
                .WithMany(u => u.Subscriptions)
                .HasForeignKey(s => s.SubscriberID)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Subscription>()
                .HasOne(s => s.SubscribedTo)
                .WithMany(u => u.Subscribers)
                .HasForeignKey(s => s.SubscribedToID)
                .OnDelete(DeleteBehavior.NoAction);

            // Video mapping to users - includes cascade delete
            modelBuilder.Entity<Video>()
                .HasOne(v => v.User)
                .WithMany(u => u.Videos)
                .HasForeignKey(v => v.UserId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Comment mapping to users - does not include cascade delete due to EF Core limitations -
            // deletion of comments will be handled when a user is deleted
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // Comment mapping to videos - includes cascade delete
            modelBuilder.Entity<Comment>()
                .HasOne(c => c.Video)
                .WithMany(v => v.Comments)
                .HasForeignKey(c => c.VideoId)
                .OnDelete(DeleteBehavior.Cascade); 

            // VideoTag mapping
            modelBuilder.Entity<VideoTag>()
                .HasKey(vt => new { vt.VideoId, vt.TagId }); // Composite key

            modelBuilder.Entity<VideoTag>()
                .HasOne(vt => vt.Video)
                .WithMany(v => v.VideoTags)
                .HasForeignKey(vt => vt.VideoId);

            modelBuilder.Entity<VideoTag>()
                .HasOne(vt => vt.Tag)
                .WithMany(t => t.VideoTags)
                .HasForeignKey(vt => vt.TagId);
        }
    }
}
