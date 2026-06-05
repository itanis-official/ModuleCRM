using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SettingsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public SettingsController(CrmDbContext db)
        {
            _db = db;
        }

        public class UserSettingDto
        {
            public string Currency { get; set; } = "TND";
            public string Language { get; set; } = "fr";
            public string TimeZone { get; set; } = "africa-tunis";
            public string DateFormat { get; set; } = "dd/mm/yyyy";
            public string Theme { get; set; } = "light";
            public string Density { get; set; } = "normal";
            public bool EmailNotifications { get; set; } = true;
            public bool PushNotifications { get; set; } = false;
            public bool WeeklyReport { get; set; } = true;
        }

        [HttpGet("me")]
        public async Task<ActionResult<UserSettingDto>> GetMine()
        {
            var userId = await User.GetLocalAgentIdAsync(_db);
            if (userId == null) return Unauthorized();

            var s = await _db.UserSettings.AsNoTracking()
                .FirstOrDefaultAsync(x => x.UserId == userId);

            if (s == null)
                return Ok(new UserSettingDto());

            return Ok(new UserSettingDto
            {
                Currency = s.Currency,
                Language = s.Language,
                TimeZone = s.TimeZone,
                DateFormat = s.DateFormat,
                Theme = s.Theme,
                Density = s.Density,
                EmailNotifications = s.EmailNotifications,
                PushNotifications = s.PushNotifications,
                WeeklyReport = s.WeeklyReport,
            });
        }

        [HttpPut("me")]
        public async Task<ActionResult<UserSettingDto>> UpdateMine([FromBody] UserSettingDto dto)
        {
            var userId = await User.GetLocalAgentIdAsync(_db);
            if (userId == null) return Unauthorized();

            var s = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserId == userId);
            if (s == null)
            {
                s = new UserSetting { UserId = userId.Value };
                _db.UserSettings.Add(s);
            }

            s.Currency = dto.Currency;
            s.Language = dto.Language;
            s.TimeZone = dto.TimeZone;
            s.DateFormat = dto.DateFormat;
            s.Theme = dto.Theme;
            s.Density = dto.Density;
            s.EmailNotifications = dto.EmailNotifications;
            s.PushNotifications = dto.PushNotifications;
            s.WeeklyReport = dto.WeeklyReport;
            s.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(dto);
        }
    }
}
