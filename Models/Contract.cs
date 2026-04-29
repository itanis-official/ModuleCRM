using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    public class Contract
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        public int? ProjectId { get; set; }

        [Required]
        [StringLength(100)]
        public string Reference { get; set; } = string.Empty;

        [Range(1, int.MaxValue)]
        public int Version { get; set; } = 1;

        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "Le montant doit être positif.")]
        public decimal Amount { get; set; } = 0;

        public string Status { get; set; } = "draft"; // draft, active, replaced, expired

        // Agent vient de ModuleRH via API
        public int? UploadedById { get; set; }

        public DateTime? UploadDate { get; set; }

        public string? Notes { get; set; }

        public string? FilePath { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
