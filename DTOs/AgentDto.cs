namespace ModuleCRM.DTOs
{
    public class AgentDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string? Avatar { get; set; }
        public string Role { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public string Poste { get; set; } = string.Empty;
        public string Departement { get; set; } = string.Empty;
        public string Statut { get; set; } = "actif";
        public string? Type { get; set; } // interne ou externe
        public string NomComplet => $"{Prenom} {Nom}";
    }
}
