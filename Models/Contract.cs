using System;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        public int? ProjectId { get; set; }
        public Project? Project { get; set; }

        [Required]
        public string Reference { get; set; } = string.Empty;

        public int Version { get; set; } = 1;

        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        public decimal Amount { get; set; } = 0;

        public string Status { get; set; } = "draft"; // draft, active, replaced, expired

        public int? UploadedById { get; set; }
        public User? UploadedByUser { get; set; }

        public DateTime? UploadDate { get; set; }

        public string? Notes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
