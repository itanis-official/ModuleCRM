using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Consumers
{
    /// <summary>
    /// Consomme AgentSyncEvent publié par ModuleRH.
    /// Maintient une table locale AgentsLocal (read-replica) afin que le CRM
    /// puisse fonctionner sans appel HTTP synchrone vers RH (machines séparées).
    /// </summary>
    public class AgentSyncConsumer : IConsumer<AgentSyncEvent>
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<AgentSyncConsumer> _logger;

        public AgentSyncConsumer(CrmDbContext db, ILogger<AgentSyncConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<AgentSyncEvent> context)
        {
            var evt = context.Message;
            var actionStr = evt.GetActionAsString();
            var agentType = string.IsNullOrWhiteSpace(evt.AgentType) ? "interne" : evt.AgentType.ToLowerInvariant();

            var existing = await _db.AgentsLocal
                .FirstOrDefaultAsync(a => a.Id == evt.Id && a.AgentType == agentType);

            if (Enum.TryParse<SyncAction>(actionStr, true, out var parsed) && parsed == SyncAction.Deleted)
            {
                if (existing != null)
                {
                    _db.AgentsLocal.Remove(existing);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("AgentLocal supprimé ({Type}/{Id})", agentType, evt.Id);
                }
                return;
            }

            if (existing != null && existing.ChangedAt > evt.ChangedAt)
            {
                _logger.LogInformation(
                    "AgentSyncEvent {Type}/{Id} ignoré (local plus récent: {Local} > {Event}).",
                    agentType, evt.Id, existing.ChangedAt, evt.ChangedAt);
                return;
            }

            if (existing == null)
            {
                _db.AgentsLocal.Add(new AgentLocal
                {
                    Id = evt.Id,
                    AgentType = agentType,
                    Nom = evt.Nom,
                    Prenom = evt.Prenom,
                    Email = evt.Email,
                    Telephone = evt.Telephone,
                    Role = evt.Role,
                    Poste = evt.Poste,
                    Departement = evt.Departement,
                    IsActive = evt.IsActive,
                    CoutHoraire = evt.CoutHoraire,
                    Rating = evt.Rating,
                    ChangedAt = evt.ChangedAt,
                    SyncedAt = DateTime.UtcNow,
                });
            }
            else
            {
                existing.Nom = evt.Nom;
                existing.Prenom = evt.Prenom;
                existing.Email = evt.Email;
                existing.Telephone = evt.Telephone;
                existing.Role = evt.Role;
                existing.Poste = evt.Poste;
                existing.Departement = evt.Departement;
                existing.IsActive = evt.IsActive;
                existing.CoutHoraire = evt.CoutHoraire;
                existing.Rating = evt.Rating;
                existing.ChangedAt = evt.ChangedAt;
                existing.SyncedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("AgentLocal {Action} ({Type}/{Id})", actionStr, agentType, evt.Id);
        }
    }
}
