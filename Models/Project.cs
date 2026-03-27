using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Project
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Reference { get; set; }
        public string? Description { get; set; }

        public string Status { get; set; } = "actif"; // actif, suspendu, termine
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Opportunity>? Opportunities { get; set; }
        public virtual ICollection<Contract>? Contracts { get; set; }
    }

}
