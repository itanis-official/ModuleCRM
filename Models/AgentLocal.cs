using System.ComponentModel.DataAnnotations.Schema;

namespace ModuleCRM.Models
{
    /// <summary>
    /// Read-replica local des Agents (collaborateurs RH).
    /// Alimenté par AgentSyncEvent publié par ModuleRH via RabbitMQ.
    /// Permet au CRM de fonctionner sans appel HTTP synchrone vers RH.
    /// </summary>
    [Table("AgentsLocal")]
    public class AgentLocal
    {
        public int Id { get; set; }
        public string AgentType { get; set; } = "interne";
        public string Nom { get; set; } = string.Empty;
        public string Prenom { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telephone { get; set; }
        public string Role { get; set; } = string.Empty;
        public string Poste { get; set; } = string.Empty;
        public string Departement { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public decimal? CoutHoraire { get; set; }
        public decimal? Rating { get; set; }
        public DateTime SyncedAt { get; set; } = DateTime.UtcNow;
        public DateTime ChangedAt { get; set; } = DateTime.UtcNow;
    }
}
