namespace VideoHost.Server.Models
{
    public class VideoTag
    {
        public int VideoId { get; set; }

        public int TagId { get; set; }

        public required Video Video { get; set; }

        public required Tag Tag { get; set; }
    }
}
