using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string RaisonSociale { get; set; } = string.Empty;

        [StringLength(50)]
        public string? MatriculeFiscal { get; set; }
        [StringLength(10)]
        public string MatriculeFiscalCountry { get; set; } = "TN";
        [StringLength(100)]
        public string? Secteur { get; set; }
        public string? Logo { get; set; }
        public string? Devis { get; set; }

        // Address
        [StringLength(300)]
        public string? Adresse { get; set; }
        [StringLength(10)]
        public string? CodePostal { get; set; }
        [StringLength(100)]
        public string? Ville { get; set; }
        [StringLength(100)]
        public string Pays { get; set; } = "Tunisie";

        // Contact
        [EmailAddress]
        [StringLength(150)]
        public string? EmailPrincipal { get; set; }
        [EmailAddress]
        [StringLength(150)]
        public string? EmailSecondaire { get; set; }
        [StringLength(30)]
        public string? TelephonePrincipal { get; set; }
        public string TelephonePrincipalCountry { get; set; } = "+216";
        public string? TelephoneSecondaire { get; set; }
        public string TelephoneSecondaireCountry { get; set; } = "+216";

        // Management (Agent vient de ModuleRH via API)
        public int? AgentResponsableId { get; set; }
        public int? EquipeResponsableId { get; set; }
        [StringLength(20)]
        public string AffectationType { get; set; } = "global"; // global | agent | equipe

        public string Statut { get; set; } = "prospect";
        public string? Notes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        [JsonIgnore]
        public virtual ICollection<Contact>? Contacts { get; set; }
        [JsonIgnore]
        public virtual ICollection<Opportunity>? Opportunities { get; set; }
        [JsonIgnore]
        public virtual ICollection<Contract>? Contracts { get; set; }
    }
}
