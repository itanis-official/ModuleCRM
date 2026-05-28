using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class AuditLog
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public int EntityId { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } = string.Empty; // Create, Update, Delete, StageChange, Approve, Reject

        public string? OldValues { get; set; }

        public string? NewValues { get; set; }

        public int? UserId { get; set; }

        [StringLength(200)]
        public string? UserName { get; set; }

        [StringLength(50)]
        public string? UserType { get; set; } // interne, externe, contact

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
