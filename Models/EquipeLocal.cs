using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ModuleCRM.Models
{
    /// <summary>
    /// Read-replica local des Équipes RH/Projet.
    /// Alimenté par EquipeSyncEvent (bidirectionnel RH ↔ GestionProjet) via RabbitMQ.
    /// </summary>
    [Table("EquipesLocal")]
    public class EquipeLocal
    {
        [Key]
        public int Id { get; set; }
        public Guid EquipeGuid { get; set; }
        public string SourceModule { get; set; } = string.Empty;
        public int IdOrigine { get; set; }
        public string Nom { get; set; } = string.Empty;
        public string Domaine { get; set; } = string.Empty;
        public string? Description { get; set; }
        public int? ChefProjetIdOrigine { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;

        public List<EquipeMembreLocal> Membres { get; set; } = new();
    }

    [Table("EquipesMembresLocal")]
    public class EquipeMembreLocal
    {
        [Key]
        public int Id { get; set; }
        public int EquipeLocalId { get; set; }
        public EquipeLocal? Equipe { get; set; }
        public int CollaborateurIdOrigine { get; set; }
        public string CollaborateurType { get; set; } = "interne";
        public string RoleDansEquipe { get; set; } = "Agent";
        public DateTime DateAffectation { get; set; } = DateTime.UtcNow;
    }
}
