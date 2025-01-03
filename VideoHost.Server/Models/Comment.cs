namespace VideoHost.Server.Models
{
    public class Comment
    {
        public int Id { get; set; }

        public required string Content { get; set; }

        public DateTime CreationDate { get; set; } = DateTime.UtcNow;

        public required int UserId { get; set; }
        public required User User { get; set; }

        public required int VideoId { get; set; }
        public required Video Video { get; set; }
    }
}
