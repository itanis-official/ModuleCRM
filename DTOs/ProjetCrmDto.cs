namespace ModuleCRM.DTOs
{
    public class ProjetCrmListItemDto
    {
        public int Id { get; set; }
        public int? OpportuniteIdOrigine { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFinPrevue { get; set; }
        public string ClientRaisonSociale { get; set; } = "N/A";
        public decimal BudgetEstime { get; set; }
        public decimal? BudgetReel { get; set; }
    }

    public class ProjetCrmDetailDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public DateTime DateDebut { get; set; }
        public DateTime DateFinPrevue { get; set; }
        public string Avancement { get; set; } = string.Empty;
        public ClientCrmDto? Client { get; set; }
    }

    public class ClientCrmDto
    {
        public int Id { get; set; }
        public string RaisonSociale { get; set; } = string.Empty;
    }

    public class ProjetPhaseDto
    {
        public int Id { get; set; }
        public string TypePhase { get; set; } = string.Empty;
        public int Ordre { get; set; }
        public List<ProjetTacheDto> Taches { get; set; } = new();
    }

    public class ProjetTacheDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public int Ordre { get; set; }
        public string? ResponsableNom { get; set; }
        public List<ProjetSousTacheDto> SousTaches { get; set; } = new();
    }

    public class ProjetSousTacheDto
    {
        public int Id { get; set; }
        public string Titre { get; set; } = string.Empty;
        public string Statut { get; set; } = string.Empty;
        public decimal DureeEstimeeHeures { get; set; }
        public string? ResponsableNom { get; set; }
        public int Ordre { get; set; }
    }
}
