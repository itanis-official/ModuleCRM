namespace ModuleCRM.DTOs
{
    public class EquipeDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Domaine { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public ChefProjetDto? ChefProjet { get; set; }
    }

    public class ChefProjetDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}
