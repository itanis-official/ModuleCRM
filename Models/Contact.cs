using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        [Required]
        [StringLength(100)]
        public string Nom { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Prenom { get; set; } = string.Empty;

        [StringLength(100)]
        public string? Poste { get; set; }

        [Required]
        [EmailAddress]
        [StringLength(150)]
        public string Email { get; set; } = string.Empty;

        [StringLength(30)]
        public string? Telephone { get; set; }
        [StringLength(10)]
        public string? TelephoneCountry { get; set; } = "+216";

        // User credentials
        [Required]
        [StringLength(100)]
        public string Login { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public bool SendEmail { get; set; } = false;
        public bool ForcePasswordChange { get; set; } = true;

        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
