using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuleCRM.Models
{
    public class Projet
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.None)]
        public int Id { get; set; }

        public int OpportuniteIdOrigine { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFinPrevue { get; set; }
        public string ClientRaisonSociale { get; set; } = string.Empty;
        public decimal BudgetEstime { get; set; }
        public decimal? BudgetReel { get; set; }
        public string Description { get; set; } = string.Empty;
        public string TypeProjet { get; set; } = string.Empty;
        public int ClientId { get; set; }

        [ForeignKey("ClientId")]
        public Company? Company { get; set; }

        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;

        public List<ProjetPhase> Phases { get; set; } = new();
    }
}
