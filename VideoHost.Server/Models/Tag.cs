namespace VideoHost.Server.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public string? Description { get; set; }

        // Navigation property for related videos
        public ICollection<VideoTag>? VideoTags { get; set; }
    }
}
