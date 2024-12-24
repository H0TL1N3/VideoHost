namespace VideoHost.Server.Models
{
    public class Tag
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        public ICollection<VideoTag>? VideoTags { get; set; }
    }
}
