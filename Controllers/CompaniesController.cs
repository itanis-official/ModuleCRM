using System.Security.Claims;
using ITANIS.SharedEvents;
using MassTransit;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Models;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<CompaniesController> _logger;
        private readonly EquipeApiService _equipeApiService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationService _notificationService;

        public CompaniesController(
            CrmDbContext db,
            ILogger<CompaniesController> logger,
            IPublishEndpoint publishEndpoint,
            INotificationService notificationService,
            EquipeApiService equipeApiService)
        {
            _db = db;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _notificationService = notificationService;
            _equipeApiService = equipeApiService;
        }

        private string NormalizeRole(string? rawRole)
        {
            if (string.IsNullOrWhiteSpace(rawRole)) return "agent";
            var token = rawRole.Normalize(System.Text.NormalizationForm.FormD).ToLowerInvariant();
            token = System.Text.RegularExpressions.Regex.Replace(token, "[^a-z0-9]", "");

            if (token.Contains("superadmin") || token == "admin" || token.Contains("administrateur")) return "super_admin";
            if (token.Contains("rh") || token.Contains("ressourceshumaines")) return "rh";
            if (token.Contains("chefprojet") || token.Contains("manager") || token.Contains("lead") || (token.Contains("chef") && token.Contains("projet"))) return "chef_projet";
            if (token.Contains("commercial")) return "agent_commercial";
            if (token.Contains("contact") || token.Contains("client")) return "contact";
            
            return "agent";
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search)
        {
            try
            {
                var query = _db.Companies.AsQueryable();

                var userRole = User.GetItanisRole();
                var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
                int.TryParse(userIdStr, out var userId);

                // Role-based filtering:
                // - Admin / Agent Commercial: see everything (NO FILTER)
                // - Agent: see assigned OR unassigned companies
                // - Chef de Projet: see team companies OR assigned OR unassigned
                
                if (userRole == "agent")
                {
                    query = query.Where(c => 
                        (c.AgentResponsableId == userId) ||
                        (c.AgentResponsableId == null && c.EquipeResponsableId == null)
                    );
                }
                else if (userRole == "chef_projet")
                {
                    var allEquipes = await _equipeApiService.GetAllAsync();
                    var myEquipeIds = allEquipes
                        .Where(e => e.ChefProjet?.Id == userId)
                        .Select(e => e.Id)
                        .ToList();

                    query = query.Where(c => 
                        (c.AffectationType == "equipe" && c.EquipeResponsableId != null && myEquipeIds.Contains(c.EquipeResponsableId.Value)) ||
                        (c.AgentResponsableId == userId) ||
                        (c.AgentResponsableId == null && c.EquipeResponsableId == null)
                    );
                }
                else if (userRole != "super_admin" && userRole != "agent_commercial" && userRole != "rh")
                {
                    // Default fallback: if role is unknown or not explicitly allowed full access, 
                    // restrict to assigned companies just in case.
                    query = query.Where(c => c.AgentResponsableId == userId);
                }

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(c => c.RaisonSociale.Contains(search) || (c.EmailPrincipal != null && c.EmailPrincipal.Contains(search)));

                // Projection sans Logo/Devis (souvent base64) — récupérés via GetById quand nécessaire.
                var slim = query
                    .OrderByDescending(c => c.CreatedAt)
                    .Select(c => new
                    {
                        c.Id, c.RaisonSociale, c.MatriculeFiscal, c.MatriculeFiscalCountry, c.Secteur,
                        c.Adresse, c.CodePostal, c.Ville, c.Pays,
                        c.EmailPrincipal, c.EmailSecondaire,
                        c.TelephonePrincipal, c.TelephonePrincipalCountry,
                        c.TelephoneSecondaire, c.TelephoneSecondaireCountry,
                        c.AgentResponsableId, c.EquipeResponsableId, c.AffectationType,
                        c.Statut, c.Notes, c.MaxHeuresTraitementTicket,
                        c.CreatedAt, c.UpdatedAt, c.IsDeleted,
                        HasLogo = c.Logo != null && c.Logo != "",
                        ContactsCount = _db.Contacts.Count(ct => ct.CompanyId == c.Id)
                    });

                if (page.HasValue && pageSize.HasValue)
                {
                    var totalCount = await query.CountAsync();
                    var items = await slim
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToListAsync();

                    return Ok(new
                    {
                        items,
                        totalCount,
                        page = page.Value,
                        pageSize = pageSize.Value
                    });
                }

                return Ok(await slim.ToListAsync());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting companies");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des sociétés." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetById(int id)
        {
            var userRole = User.GetItanisRole();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            int.TryParse(userIdStr, out var userId);

            var company = await _db.Companies
                .Include(c => c.Contacts)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return NotFound();

            // Full access roles
            if (userRole == "super_admin" || userRole == "agent_commercial" || userRole == "rh")
            {
                return Ok(company);
            }

            // If unassigned, visible to all authenticated users
            if (company.AgentResponsableId == null && company.EquipeResponsableId == null)
            {
                return Ok(company);
            }

            // Restricted access check for Agent / Chef de Projet
            if (userRole == "agent" && company.AgentResponsableId != userId)
            {
                return Forbid("Vous n'avez pas accès à cette société.");
            }
            
            if (userRole == "chef_projet")
            {
                bool isOwnAgent = company.AgentResponsableId == userId;
                bool isOwnEquipe = false;
                
                if (company.AffectationType == "equipe" && company.EquipeResponsableId != null)
                {
                    var allEquipes = await _equipeApiService.GetAllAsync();
                    isOwnEquipe = allEquipes.Any(e => e.Id == company.EquipeResponsableId && e.ChefProjet?.Id == userId);
                }
                
                if (!isOwnAgent && !isOwnEquipe)
                {
                    return Forbid("Vous n'avez pas accès à cette société.");
                }
            }

            return Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult<Company>> Create(Company company)
        {
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent créer des sociétés.");
            }

            NormalizeAssignment(company);
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            await PublishSync(company, SyncAction.Created);

            await SendAssignmentNotification(company);

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Company>> Update(int id, Company updatedCompany)
        {
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent modifier des sociétés.");
            }

            var existing = await _db.Companies.FindAsync(id);
            if (existing == null)
                return NotFound();

            _logger.LogInformation(
                "[Company.Update] AVANT normalize  id={Id} affectationType={Type} agentResponsableId={AgentId} equipeResponsableId={EquipeId}",
                id, updatedCompany.AffectationType, updatedCompany.AgentResponsableId, updatedCompany.EquipeResponsableId);

            NormalizeAssignment(updatedCompany);

            _logger.LogInformation(
                "[Company.Update] APRES normalize id={Id} affectationType={Type} agentResponsableId={AgentId} equipeResponsableId={EquipeId}",
                id, updatedCompany.AffectationType, updatedCompany.AgentResponsableId, updatedCompany.EquipeResponsableId);

            existing.RaisonSociale = updatedCompany.RaisonSociale;
            existing.MatriculeFiscal = updatedCompany.MatriculeFiscal;
            existing.MatriculeFiscalCountry = updatedCompany.MatriculeFiscalCountry;
            existing.Secteur = updatedCompany.Secteur;
            existing.Logo = updatedCompany.Logo;
            existing.Devis = updatedCompany.Devis;
            existing.Adresse = updatedCompany.Adresse;
            existing.CodePostal = updatedCompany.CodePostal;
            existing.Ville = updatedCompany.Ville;
            existing.Pays = updatedCompany.Pays;
            existing.EmailPrincipal = updatedCompany.EmailPrincipal;
            existing.EmailSecondaire = updatedCompany.EmailSecondaire;
            existing.TelephonePrincipal = updatedCompany.TelephonePrincipal;
            existing.TelephonePrincipalCountry = updatedCompany.TelephonePrincipalCountry;
            existing.TelephoneSecondaire = updatedCompany.TelephoneSecondaire;
            existing.TelephoneSecondaireCountry = updatedCompany.TelephoneSecondaireCountry;
            existing.AgentResponsableId = updatedCompany.AgentResponsableId;
            existing.EquipeResponsableId = updatedCompany.EquipeResponsableId;
            existing.AffectationType = updatedCompany.AffectationType;
            existing.Statut = updatedCompany.Statut;
            existing.Notes = updatedCompany.Notes;
            existing.MaxHeuresTraitementTicket = updatedCompany.MaxHeuresTraitementTicket;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            await PublishSync(existing, SyncAction.Updated);

            await SendAssignmentNotification(existing);

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent supprimer des sociétés.");
            }

            var existing = await _db.Companies.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Companies.Remove(existing);
            await _db.SaveChangesAsync();

            await PublishSync(existing, SyncAction.Deleted);

            return NoContent();
        }

        private Task PublishSync(Company c, SyncAction action)
        {
            var agentToPublish = c.AffectationType == "agent" ? c.AgentResponsableId : null;

            _logger.LogInformation(
                "[Company.PublishSync] action={Action} id={Id} affectationType={Type} agentResponsableId={AgentId} -> publié AgentResponsableId={Published}",
                action, c.Id, c.AffectationType, c.AgentResponsableId, agentToPublish);

            return _publishEndpoint.Publish(new CompanySyncEvent
            {
                Action = action,
                Id = c.Id,
                RaisonSociale = c.RaisonSociale,
                Secteur = c.Secteur,
                EmailPrincipal = c.EmailPrincipal,
                TelephonePrincipal = c.TelephonePrincipal,
                Adresse = c.Adresse,
                CodePostal = c.CodePostal,
                Ville = c.Ville,
                Pays = c.Pays,
                MatriculeFiscal = c.MatriculeFiscal,
                Statut = c.Statut,
                MaxHeuresTraitementTicket = c.MaxHeuresTraitementTicket,
                AgentResponsableId = agentToPublish,
                ChangedAt = DateTime.UtcNow,
            });
        }

        private static void NormalizeAssignment(Company company)
        {
            var type = (company.AffectationType ?? "global").Trim().ToLowerInvariant();
            if (type != "agent" && type != "equipe" && type != "global")
            {
                type = "global";
            }

            company.AffectationType = type;

            if (type == "agent")
            {
                if (!company.AgentResponsableId.HasValue || company.AgentResponsableId.Value <= 0)
                {
                    company.AffectationType = "global";
                    company.AgentResponsableId = null;
                }
                company.EquipeResponsableId = null;
                return;
            }

            if (type == "equipe")
            {
                if (!company.EquipeResponsableId.HasValue || company.EquipeResponsableId.Value <= 0)
                {
                    company.AffectationType = "global";
                    company.EquipeResponsableId = null;
                }
                company.AgentResponsableId = null;
                return;
            }

            company.AgentResponsableId = null;
            company.EquipeResponsableId = null;
        }

        private async Task SendAssignmentNotification(Company company)
        {
            try
            {
                int? targetUserId = null;
                string assignedToName = "";

                if (company.AffectationType == "agent" && company.AgentResponsableId.HasValue)
                {
                    targetUserId = company.AgentResponsableId.Value;
                    assignedToName = "vous a été affectée";
                }
                else if (company.AffectationType == "equipe" && company.EquipeResponsableId.HasValue)
                {
                    var equipe = await _equipeApiService.GetByIdAsync(company.EquipeResponsableId.Value);
                    if (equipe?.ChefProjet != null)
                    {
                        targetUserId = equipe.ChefProjet.Id;
                        assignedToName = $"a été affectée à votre équipe ({equipe.Nom})";
                    }
                }

                if (targetUserId.HasValue)
                {
                    var title = "Nouvelle affectation de société";
                    var message = $"La société {company.RaisonSociale} {assignedToName}.";
                    if (!string.IsNullOrWhiteSpace(company.Notes))
                    {
                        message += $"\n\nNotes: {company.Notes}";
                    }

                    await _notificationService.NotifyAsync(
                        targetUserId.Value,
                        "assignment",
                        title,
                        message,
                        $"/crm/companies/{company.Id}"
                    );

                    _logger.LogInformation("Notification d'affectation envoyée à l'utilisateur {UserId} pour la société {CompanyId}", targetUserId, company.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'envoi de la notification d'affectation pour la société {CompanyId}", company.Id);
            }
        }
    }
}
