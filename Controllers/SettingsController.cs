using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

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

        // Cle stable de l'utilisateur depuis le token Authentik : email en priorite, sinon le sub.
        // Important : le sub Authentik est un UUID (pas un int) -> on le garde en string.
        // Independant de la replique AgentsLocal : marche pour tout utilisateur authentifie.
        private string? GetUserKey()
            => User.FindFirstValue(ClaimTypes.Email)
            ?? User.FindFirstValue("email")
            ?? User.FindFirstValue("preferred_username")
            ?? User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");

        [HttpGet("me")]
        public async Task<ActionResult<UserSettingDto>> GetMine()
        {
            var key = GetUserKey();
            var s = string.IsNullOrWhiteSpace(key)
                ? null
                : await _db.UserSettings.AsNoTracking().FirstOrDefaultAsync(x => x.UserKey == key);

            if (s == null)
                return Ok(new UserSettingDto()); // pas de reglages encore : valeurs par defaut (jamais 401)

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
            var key = GetUserKey();
            if (string.IsNullOrWhiteSpace(key))
                return Ok(dto); // token sans identite exploitable : rien a persister, mais pas d'erreur

            var s = await _db.UserSettings.FirstOrDefaultAsync(x => x.UserKey == key);
            if (s == null)
            {
                s = new UserSetting { UserKey = key };
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

        // ===== Reglages GLOBAUX (clef -> valeur JSON), partages par tous les utilisateurs =====
        // Ex: 'select-options-config' = configuration des listes de parametres CRM.
        // N'exige que l'authentification (pas l'id agent local) : lisible par tout employe,
        // car les formulaires consomment ces listes.
        public class AppSettingValueDto
        {
            public string? Value { get; set; }
        }

        [HttpGet("app/{key}")]
        public async Task<ActionResult<AppSettingValueDto>> GetApp(string key)
        {
            var s = await _db.AppSettings.AsNoTracking().FirstOrDefaultAsync(x => x.Key == key);
            return Ok(new AppSettingValueDto { Value = s?.Value });
        }

        [HttpPut("app/{key}")]
        public async Task<ActionResult<AppSettingValueDto>> PutApp(string key, [FromBody] AppSettingValueDto dto)
        {
            var s = await _db.AppSettings.FirstOrDefaultAsync(x => x.Key == key);
            if (s == null)
            {
                s = new AppSetting { Key = key };
                _db.AppSettings.Add(s);
            }
            s.Value = dto.Value ?? string.Empty;
            s.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            return Ok(new AppSettingValueDto { Value = s.Value });
        }
    }
}
