using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using System.Security.Claims;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public NotificationsController(CrmDbContext db)
        {
            _db = db;
        }

        private int? GetCurrentUserId()
        {
            var sub = User.FindFirst("sub")?.Value ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(sub, out var id) ? id : null;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] int take = 30)
        {
            var userId = GetCurrentUserId();
            take = Math.Clamp(take, 1, 100);

            var items = await _db.Notifications
                .AsNoTracking()
                .Where(n => n.UserId == userId || n.UserId == null)
                .OrderByDescending(n => n.CreatedAt)
                .Take(take)
                .Select(n => new
                {
                    n.Id,
                    n.Type,
                    n.Title,
                    n.Message,
                    n.Link,
                    n.IsRead,
                    n.CreatedAt,
                    n.ReadAt,
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = GetCurrentUserId();
            var count = await _db.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .CountAsync();
            return Ok(new { count });
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = GetCurrentUserId();
            var n = await _db.Notifications.FirstOrDefaultAsync(x =>
                x.Id == id && (x.UserId == userId || x.UserId == null));
            if (n == null) return NotFound();
            if (!n.IsRead)
            {
                n.IsRead = true;
                n.ReadAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
            return NoContent();
        }

        [HttpPost("read-all")]
        public async Task<IActionResult> MarkAllRead()
        {
            var userId = GetCurrentUserId();
            var now = DateTime.UtcNow;
            var items = await _db.Notifications
                .Where(n => (n.UserId == userId || n.UserId == null) && !n.IsRead)
                .ToListAsync();
            foreach (var n in items)
            {
                n.IsRead = true;
                n.ReadAt = now;
            }
            await _db.SaveChangesAsync();
            return Ok(new { updated = items.Count });
        }
    }
}
