using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using VideoHost.Server.Data;
using VideoHost.Server.Models;

namespace VideoHost.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class SubscriptionController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly ApplicationDbContext _dbContext;

        public SubscriptionController(UserManager<User> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _dbContext = context;
        }

        [HttpGet("get")]
        public async Task<IActionResult> Get(int subscribedToId)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
                return NotFound(new { message = "Subscriber is required." });

            var subscription = await _dbContext.Subscriptions
                .Include(s => s.SubscribedTo)
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriber.Id && s.SubscribedToId == subscribedToId);

            if (subscription == null)
                return Ok();

            return Ok(new
            {
                subscription!.Id,
                Subscriber = new {
                    subscription.Subscriber.Id,
                    subscription.Subscriber.DisplayName
                },
                SubscribedTo = new
                {
                    subscription.SubscribedTo.Id,
                    subscription.SubscribedTo.DisplayName
                },
                subscription.SubscriptionDate
            });
        }

        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] SubscriptionAddRequest request)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null) 
                return NotFound(new { message = "Subscriber is required." });

            var subscribedTo = await _userManager.FindByIdAsync(request.SubscribedToId.ToString());
            if (subscribedTo == null)
                return NotFound(new { message = "Subscribed To is required." });

            var subscription = new Subscription
            {
                SubscriberId = subscriber.Id,
                Subscriber = subscriber,          
                SubscribedToId = subscribedTo.Id,
                SubscribedTo = subscribedTo,
                SubscriptionDate = DateTime.UtcNow
            };

            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "You have subscribed successfully!" });
        }
        
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int subscribedToId)
        {
            var subscriber = await _userManager.GetUserAsync(User);
            if (subscriber == null)
                return NotFound(new { message = "Subscriber is required." });

            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriber.Id && s.SubscribedToId == subscribedToId);

            if (subscription == null)
                return NotFound(new { message = "Subscription does not exist." });

            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "You have unsubscribed successfully." });
        }
    }

    public class SubscriptionAddRequest
    {
        public required int SubscribedToId { get; set; }
    }
}
