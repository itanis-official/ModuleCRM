using System.Globalization;
using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Consumers
{
    public class ProjetSyncConsumer : IConsumer<ProjetSyncEvent>
    {
        private readonly CrmDbContext _db;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly ILogger<ProjetSyncConsumer> _logger;

        public ProjetSyncConsumer(CrmDbContext db, ILogger<ProjetSyncConsumer> logger, IPublishEndpoint publishEndpoint)
        {
            _db = db;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        public async Task Consume(ConsumeContext<ProjetSyncEvent> context)
        {
            var msg = context.Message;

            var fr = CultureInfo.GetCultureInfo("fr-FR");
            var budgetEstimeFmt = msg.BudgetEstime.ToString("N2", fr);
            var budgetReelFmt = msg.BudgetReel.HasValue
                ? msg.BudgetReel.Value.ToString("N2", fr)
                : "NULL";

            _logger.LogInformation(
                "ProjetSyncEvent reçu: action={Action} id={Id} nom={Nom} phases={PhaseCount} budgetEstime={BudgetEstime} budgetReel={BudgetReel} budgetReelHasValue={HasValue}",
                msg.Action, msg.Id, msg.Nom, msg.Phases?.Count ?? 0,
                budgetEstimeFmt,
                budgetReelFmt,
                msg.BudgetReel.HasValue);

            if (msg.Action == SyncAction.Deleted)
            {
                var existing = await _db.Projets.FindAsync(msg.Id);
                if (existing != null)
                {
                    int clientId = existing.ClientId;
                    _db.Projets.Remove(existing);
                    await _db.SaveChangesAsync();
                    await UpdateCompanyStatusAsync(clientId);
                }
                return;
            }

            var entity = await _db.Projets
                .Include(p => p.Phases)
                    .ThenInclude(ph => ph.Taches)
                        .ThenInclude(t => t.SousTaches)
                .FirstOrDefaultAsync(p => p.Id == msg.Id);

            if (entity == null)
            {
                entity = new Projet { Id = msg.Id };
                _db.Projets.Add(entity);
            }

            entity.OpportuniteIdOrigine = msg.OpportuniteIdOrigine;
            entity.Nom = msg.Nom;
            entity.Statut = msg.Statut;
            entity.DateDebut = msg.DateDebut;
            entity.DateFinPrevue = msg.DateFinPrevue;
            entity.ClientRaisonSociale = msg.ClientRaisonSociale;
            entity.BudgetEstime = msg.BudgetEstime;
            entity.BudgetReel = msg.BudgetReel;
            entity.Description = msg.Description;
            entity.TypeProjet = msg.TypeProjet;
            entity.ClientId = msg.ClientId;
            entity.SyncedAt = DateTime.UtcNow;

            // Replace hierarchy wholesale — Nesrine sends the full state each sync
            if (entity.Phases.Count > 0)
            {
                _db.ProjetPhases.RemoveRange(entity.Phases);
                entity.Phases = new List<ProjetPhase>();
            }

            var phaseIndex = 0;
            foreach (var phaseDto in msg.Phases ?? new List<PhaseSyncDto>())
            {
                var phase = new ProjetPhase
                {
                    TypePhase = phaseDto.TypePhase,
                    Ordre = phaseIndex++,
                };

                var tacheIndex = 0;
                foreach (var tacheDto in phaseDto.Taches ?? new List<TacheSyncDto>())
                {
                    var tache = new ProjetTache
                    {
                        Titre = tacheDto.Titre,
                        Statut = tacheDto.Statut,
                        ResponsableNom = tacheDto.ResponsableNom,
                        ResponsableId = tacheDto.ResponsableId,
                        Ordre = tacheIndex++,
                    };

                    var sousTacheIndex = 0;
                    foreach (var sousTacheDto in tacheDto.SousTaches ?? new List<SousTacheSyncDto>())
                    {
                        tache.SousTaches.Add(new ProjetSousTache
                        {
                            Titre = sousTacheDto.Titre,
                            Statut = sousTacheDto.Statut,
                            DureeEstimeeHeures = sousTacheDto.DureeEstimeeHeures,
                            ResponsableNom = sousTacheDto.ResponsableNom,
                            ResponsableId = sousTacheDto.ResponsableId,
                            Ordre = sousTacheIndex++,
                        });
                    }

                    phase.Taches.Add(tache);
                }

                entity.Phases.Add(phase);
            }

            await _db.SaveChangesAsync();

            await UpdateCompanyStatusAsync(entity.ClientId);
        }

        private async Task UpdateCompanyStatusAsync(int companyId)
        {
            var company = await _db.Companies.FindAsync(companyId);
            if (company == null) return;

            var projets = await _db.Projets
                .Where(p => p.ClientId == companyId)
                .ToListAsync();

            if (projets.Count == 0)
            {
                company.Statut = "prospect";
            }
            else if (projets.All(p => p.Statut == "Termine"))
            {
                company.Statut = "inactif";
            }
            else
            {
                company.Statut = "client";
            }

            await _db.SaveChangesAsync();

            await _publishEndpoint.Publish(new CompanySyncEvent
            {
                Action = SyncAction.Updated,
                Id = company.Id,
                RaisonSociale = company.RaisonSociale,
                Secteur = company.Secteur,
                EmailPrincipal = company.EmailPrincipal,
                TelephonePrincipal = company.TelephonePrincipal,
                Adresse = company.Adresse,
                CodePostal = company.CodePostal,
                Ville = company.Ville,
                Pays = company.Pays,
                MatriculeFiscal = company.MatriculeFiscal,
                Statut = company.Statut,
                MaxHeuresTraitementTicket = company.MaxHeuresTraitementTicket,
                ChangedAt = DateTime.UtcNow,
            });

            _logger.LogInformation("Statut de la société ID={CompanyId} mis à jour et synchronisé : {Statut}", companyId, company.Statut);
        }
    }
}
