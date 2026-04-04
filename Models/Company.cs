using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class Company
    {
        public int Id { get; set; }

        [Required]
        public string RaisonSociale { get; set; } = string.Empty;

        public string? MatriculeFiscal { get; set; }
        public string MatriculeFiscalCountry { get; set; } = "TN";
        public string? Secteur { get; set; }
        public string? Logo { get; set; }
        public string? Devis { get; set; }

        // Address
        public string? Adresse { get; set; }
        public string? CodePostal { get; set; }
        public string? Ville { get; set; }
        public string Pays { get; set; } = "Tunisie";

        // Contact
        public string? EmailPrincipal { get; set; }
        public string? EmailSecondaire { get; set; }
        public string? TelephonePrincipal { get; set; }
        public string TelephonePrincipalCountry { get; set; } = "+216";
        public string? TelephoneSecondaire { get; set; }
        public string TelephoneSecondaireCountry { get; set; } = "+216";

        // Management (Agent vient de ModuleRH via API)
        public int? AgentResponsableId { get; set; }

        public string Statut { get; set; } = "prospect";
        public string? Notes { get; set; }

        // Timestamps
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        // Navigation
        public virtual ICollection<Contact>? Contacts { get; set; }
        public virtual ICollection<Opportunity>? Opportunities { get; set; }
        public virtual ICollection<Contract>? Contracts { get; set; }
    }
}
