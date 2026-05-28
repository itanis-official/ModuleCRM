using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Consumers
{
    /// <summary>
    /// Consomme EquipeSyncEvent (publié par RH ou GestionProjet).
    /// Le CRM ne fait pas autorité sur les équipes : il maintient un read-replica
    /// indexé par EquipeGuid, sans republier d'événement (consumer purement passif).
    /// </summary>
    public class EquipeSyncConsumer : IConsumer<EquipeSyncEvent>
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<EquipeSyncConsumer> _logger;

        public EquipeSyncConsumer(CrmDbContext db, ILogger<EquipeSyncConsumer> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<EquipeSyncEvent> context)
        {
            var evt = context.Message;

            if (evt.EquipeGuid == Guid.Empty)
            {
                _logger.LogWarning("EquipeSyncEvent reçu sans EquipeGuid (source {Source}) — ignoré.", evt.SourceModule);
                return;
            }

            var existing = await _db.EquipesLocal
                .Include(e => e.Membres)
                .FirstOrDefaultAsync(e => e.EquipeGuid == evt.EquipeGuid);

            var actionStr = evt.GetActionAsString();
            if (Enum.TryParse<SyncAction>(actionStr, true, out var parsed) && parsed == SyncAction.Deleted)
            {
                if (existing != null)
                {
                    _db.EquipesMembresLocal.RemoveRange(existing.Membres);
                    _db.EquipesLocal.Remove(existing);
                    await _db.SaveChangesAsync();
                    _logger.LogInformation("EquipeLocal {Guid} supprimée (source {Source}).", evt.EquipeGuid, evt.SourceModule);
                }
                return;
            }

            if (existing != null && existing.ChangedAt > evt.ChangedAt)
            {
                _logger.LogInformation(
                    "EquipeSyncEvent {Guid} ignoré (local plus récent: {Local} > {Event}).",
                    evt.EquipeGuid, existing.ChangedAt, evt.ChangedAt);
                return;
            }

            if (existing == null)
            {
                existing = new EquipeLocal
                {
                    EquipeGuid = evt.EquipeGuid,
                    SourceModule = evt.SourceModule,
                    IdOrigine = evt.Id,
                    Nom = evt.Nom,
                    Domaine = evt.Domaine,
                    Description = evt.Description,
                    ChefProjetIdOrigine = evt.ChefProjetIdOrigine,
                    IsActive = evt.IsActive,
                    ChangedAt = evt.ChangedAt,
                    SyncedAt = DateTime.UtcNow,
                };
                _db.EquipesLocal.Add(existing);
                await _db.SaveChangesAsync();
            }
            else
            {
                existing.SourceModule = evt.SourceModule;
                existing.IdOrigine = evt.Id;
                existing.Nom = evt.Nom;
                existing.Domaine = evt.Domaine;
                existing.Description = evt.Description;
                existing.ChefProjetIdOrigine = evt.ChefProjetIdOrigine;
                existing.IsActive = evt.IsActive;
                existing.ChangedAt = evt.ChangedAt;
                existing.SyncedAt = DateTime.UtcNow;
            }

            // Replace les membres complètement (last-write-wins)
            _db.EquipesMembresLocal.RemoveRange(existing.Membres);
            existing.Membres.Clear();
            foreach (var m in evt.Membres ?? new List<EquipeMembreSyncDto>())
            {
                existing.Membres.Add(new EquipeMembreLocal
                {
                    EquipeLocalId = existing.Id,
                    CollaborateurIdOrigine = m.CollaborateurIdOrigine,
                    CollaborateurType = string.IsNullOrWhiteSpace(m.CollaborateurType)
                        ? "interne"
                        : m.CollaborateurType.ToLowerInvariant(),
                    RoleDansEquipe = m.RoleDansEquipe,
                    DateAffectation = m.DateAffectation,
                });
            }

            await _db.SaveChangesAsync();
            _logger.LogInformation("EquipeLocal {Guid} ({Action}) appliquée depuis {Source}.",
                evt.EquipeGuid, actionStr, evt.SourceModule);
        }
    }
}
