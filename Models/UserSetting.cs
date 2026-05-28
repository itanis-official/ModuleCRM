using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class UserSetting
    {
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [StringLength(8)]
        public string Currency { get; set; } = "TND";

        [StringLength(8)]
        public string Language { get; set; } = "fr";

        [StringLength(64)]
        public string TimeZone { get; set; } = "africa-tunis";

        [StringLength(16)]
        public string DateFormat { get; set; } = "dd/mm/yyyy";

        [StringLength(16)]
        public string Theme { get; set; } = "light";

        [StringLength(16)]
        public string Density { get; set; } = "normal";

        public bool EmailNotifications { get; set; } = true;
        public bool PushNotifications { get; set; } = false;
        public bool WeeklyReport { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
