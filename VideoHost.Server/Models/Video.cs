namespace VideoHost.Server.Models
{
    public class Video
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public required string UploadPath { get; set; }

        public required string ThumbnailPath { get; set; }

        public DateTime UploadDate { get; set; } = DateTime.UtcNow;

        public string? Description { get; set; }

        public int ViewCount { get; set; }

        public required int UserId { get; set; }

        public required User User { get; set; }

        public ICollection<VideoTag>? VideoTags { get; set; }

        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
    }
}
