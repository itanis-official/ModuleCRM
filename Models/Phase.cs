using System;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Phase
    {
        public int Id { get; set; }

        public int OpportunityId { get; set; }
        public Opportunity? Opportunity { get; set; }

        [Required]
        public string Type { get; set; } = "meeting"; // meeting, study, offer, contract

        public string Status { get; set; } = "pending"; // pending, in_progress, completed, not_sent, sent, accepted, negotiated, refused

        public string? Notes { get; set; }

        // Phase Reunion
        public DateTime? MeetingDate { get; set; }
        public string? MeetingTime { get; set; }

        // Phase Etude (Agent vient de ModuleRH via API)
        public int? AgentEtudeId { get; set; }
        public DateTime? DueDate { get; set; }
        public int Progress { get; set; } = 0;
        public bool Validated { get; set; } = false;

        // Phase Offre
        public decimal? Montant { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public DateTime? DateValidite { get; set; }
        public string? FeedbackClient { get; set; }

        // Phase Contrat
        public string? Reference { get; set; }
        public DateTime? DateSignature { get; set; }
        public bool Signed { get; set; } = false;

        // Documents
        public string? DocumentPath { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
