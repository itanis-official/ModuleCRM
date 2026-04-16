namespace ModuleCRM.DTOs
{
    public class EquipeDto
    {
        public int Id { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Domaine { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
    }
}
