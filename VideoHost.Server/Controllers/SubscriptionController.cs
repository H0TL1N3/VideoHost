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
        public async Task<IActionResult> Get(int subscriberId, int subscribedToId)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == subscribedToId);

            return Ok(subscription);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> Add([FromBody] SubscriptionAddRequest request)
        {
            var subscriber = await _userManager.FindByIdAsync(request.SubscriberId.ToString());
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
                SubscriptionDate = DateTime.Now
            };

            _dbContext.Subscriptions.Add(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "You have subscribed successfully!" });
        }

        [Authorize]
        [HttpDelete("delete")]
        public async Task<IActionResult> Delete(int subscriberId, int subscribedToId)
        {
            var subscription = await _dbContext.Subscriptions
                .FirstOrDefaultAsync(s => s.SubscriberId == subscriberId && s.SubscribedToId == subscribedToId);

            if (subscription == null)
                return NotFound(new { message = "Subscription does not exist." });

            _dbContext.Subscriptions.Remove(subscription);
            await _dbContext.SaveChangesAsync();

            return Ok(new { message = "You have unsubscribed successfully!" });
        }
    }

    public class SubscriptionAddRequest
    {
        public required int SubscriberId { get; set; }
        public required int SubscribedToId { get; set; }
    }
}
