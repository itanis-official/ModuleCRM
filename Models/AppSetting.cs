using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    // Reglage global de l'application (clef -> valeur JSON), partage par tous les utilisateurs.
    // Sert notamment a persister la configuration des listes de parametres CRM,
    // qui etait auparavant stockee uniquement dans le backend RH / le localStorage.
    public class AppSetting
    {
        [Key]
        [MaxLength(100)]
        public string Key { get; set; } = string.Empty;

        // Contenu JSON serialise.
        public string Value { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
