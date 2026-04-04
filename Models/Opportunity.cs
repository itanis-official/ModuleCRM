using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Opportunity
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        public Company? Company { get; set; }

        public int? ProjectParentId { get; set; }

        [Required]
        public string Titre { get; set; } = string.Empty;

        public string? Description { get; set; }

        public decimal? ValeurEstimee { get; set; }
        public int Probabilite { get; set; } = 50;

        public string PipelineStage { get; set; } = "prospection";
        public DateTime? DateCloturePrevu { get; set; }
        public DateTime? DateCloture { get; set; }

        public string Type { get; set; } = "nouveau";
        public string? SubType { get; set; }

        // Agents viennent de ModuleRH via API
        public int? AgentCommercialId { get; set; }
        public int? AgentCdcId { get; set; }
        public DateTime? EcheanceCdc { get; set; }
        public string? CdcFilePath { get; set; }

        public string? RaisonPerte { get; set; }
        public string? Notes { get; set; }

        // Phases
        public virtual ICollection<Phase>? Phases { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
