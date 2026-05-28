using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    /// <summary>
    /// Phase d'une opportunité commerciale. Le type dépend de l'opportunité :
    /// - Projet normal : meeting → study → offer → contract
    /// - Helpdesk      : proposition → contract
    /// </summary>
    public class Phase
    {
        public int Id { get; set; }

        public int OpportunityId { get; set; }
        [JsonIgnore]
        public Opportunity? Opportunity { get; set; }

        /// <summary>
        /// meeting | study | offer | contract | proposition
        /// </summary>
        [Required]
        public string Type { get; set; } = "meeting";

        /// <summary>
        /// Ordre stable d'affichage dans le stepper.
        /// </summary>
        public int Order { get; set; } = 0;

        /// <summary>
        /// Statuts par type :
        ///   meeting     : pending | scheduled | completed
        ///   study       : pending | in_progress | completed
        ///   offer       : not_sent | sent | accepted | refused | negotiated
        ///   proposition : not_sent | sent | accepted | refused
        ///   contract    : pending | signed
        /// </summary>
        public string Status { get; set; } = "pending";

        public string? Notes { get; set; }

        /// <summary>
        /// Agent responsable de la phase (étude/proposition principalement).
        /// Référence vers ModuleRH via API.
        /// </summary>
        public int? AgentResponsableId { get; set; }

        // Phase study : avancement et validation client
        public DateTime? DueDate { get; set; }
        public int Progress { get; set; } = 0;
        public bool Validated { get; set; } = false;

        // Phase offer / proposition : montant + dates + retour
        public decimal? Montant { get; set; }
        public DateTime? DateEnvoi { get; set; }
        public DateTime? DateValidite { get; set; }
        public string? FeedbackClient { get; set; }

        // Document attaché (proposition de contrat pour helpdesk, document d'étude pour study).
        // Le CDC client lui reste sur Opportunity.CdcFilePath (cf. règle de design).
        public string? DocumentPath { get; set; }
        public string? DocumentName { get; set; }
        public string? DocumentContentType { get; set; }

        // Réunions multiples (phase meeting uniquement)
        public List<Meeting> Meetings { get; set; } = new();

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
