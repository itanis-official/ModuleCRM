using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    public class ProjetPhase
    {
        [Key]
        public int Id { get; set; }

        public int ProjetId { get; set; }
        [JsonIgnore]
        public Projet? Projet { get; set; }

        public string TypePhase { get; set; } = string.Empty;
        public int Ordre { get; set; }

        public List<ProjetTache> Taches { get; set; } = new();
    }

    public class ProjetTache
    {
        [Key]
        public int Id { get; set; }

        public int ProjetPhaseId { get; set; }
        [JsonIgnore]
        public ProjetPhase? Phase { get; set; }

        public string Titre { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int Ordre { get; set; }
        public string? ResponsableNom { get; set; }
        public int? ResponsableId { get; set; }

        public List<ProjetSousTache> SousTaches { get; set; } = new();
    }

    public class ProjetSousTache
    {
        [Key]
        public int Id { get; set; }

        public int ProjetTacheId { get; set; }
        [JsonIgnore]
        public ProjetTache? Tache { get; set; }

        public string Titre { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public decimal DureeEstimeeHeures { get; set; }
        public string? ResponsableNom { get; set; }
        public int? ResponsableId { get; set; }
        public int Ordre { get; set; }
    }
}
