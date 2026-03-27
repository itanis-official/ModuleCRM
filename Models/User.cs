using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required]
        public string Prenom { get; set; } = string.Empty;

        [Required]
        public string Email { get; set; } = string.Empty;

        public string? Telephone { get; set; }

        [Required]
        public string Login { get; set; } = string.Empty;

        public string PasswordHash { get; set; } = string.Empty;

        public string? Avatar { get; set; }
        public string Role { get; set; } = "admin";

        public bool IsActive { get; set; } = true;
        public DateTime? LastLogin { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Company>? CompaniesManaged { get; set; }
        public virtual ICollection<Opportunity>? OpportunitiesManaged { get; set; }
    }
}
