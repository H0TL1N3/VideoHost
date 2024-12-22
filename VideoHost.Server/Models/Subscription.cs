namespace VideoHost.Server.Models
{
    public class Subscription
    {
        public int Id { get; set; }

        public required int SubscriberId { get; set; }
        public required User Subscriber { get; set; }

        public required int SubscribedToId { get; set; }
        public required User SubscribedTo { get; set; }

        public DateTime SubscriptionDate { get; set; } = DateTime.UtcNow;
    }
}
