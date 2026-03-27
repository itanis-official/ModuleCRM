using System;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Contact
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        public string? Poste { get; set; }

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? Telephone { get; set; }
        public string? TelephoneCountry { get; set; } = "+216";

        // User credentials
        [Required]
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
