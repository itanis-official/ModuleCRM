using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    public class Opportunity
    {
        public int Id { get; set; }

        public int CompanyId { get; set; }
        [JsonIgnore]
        public Company? Company { get; set; }

        public int? ProjectParentId { get; set; }

        [Required]
        [StringLength(200)]
        public string Titre { get; set; } = string.Empty;

        [StringLength(2000)]
        public string? Description { get; set; }

        [Range(0, double.MaxValue, ErrorMessage = "La valeur estimée doit être positive.")]
        public decimal? ValeurEstimee { get; set; }

        /// <summary>
        /// Code ISO 4217 (TND, EUR, USD, GBP) — devise dans laquelle ValeurEstimee est exprimée.
        /// </summary>
        [StringLength(3)]
        public string? Devise { get; set; }

        [Range(0, 100, ErrorMessage = "La probabilité doit être entre 0 et 100.")]
        public int Probabilite { get; set; } = 50;

        public string PipelineStage { get; set; } = "prospection";
        public DateTime? DateCloturePrevu { get; set; }
        public DateTime? DateCloture { get; set; }

        public string Type { get; set; } = "nouveau";
        public string? SubType { get; set; }
        public string? TypeProjet { get; set; }

        // Agents viennent de ModuleRH via API
        public int? AgentCommercialId { get; set; }
        public int? AgentCdcId { get; set; }
        public DateTime? EcheanceCdc { get; set; }
        public string? CdcFilePath { get; set; }
        public string? CdcFileName { get; set; }
        public string? CdcContentType { get; set; }

        // Pour les opportunités helpdesk : pas de CDC, mais une proposition de contrat
        public string? PropositionContratFilePath { get; set; }
        public string? PropositionContratFileName { get; set; }
        public string? PropositionContratContentType { get; set; }

        // Équipe assignée lors de la conversion en projet (FK soft vers Equipe côté RH)
        public int? EquipeId { get; set; }

        public string? RaisonPerte { get; set; }
        public string? Notes { get; set; }

        // Phases
        [JsonIgnore]
        public virtual ICollection<Phase>? Phases { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsDeleted { get; set; } = false;
    }
}
