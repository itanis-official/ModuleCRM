using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [Authorize(Roles = "contact")]
    [ApiController]
    [Route("api/[controller]")]
    public class ClientPortalController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public ClientPortalController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ClientPortalDashboardDto>> GetDashboard()
        {
            var contactId = await GetContactIdAsync();
            if (contactId == null)
                return Unauthorized(new { message = "Contact introuvable dans le token." });

            var contact = await _db.Contacts
                .AsNoTracking()
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == contactId.Value && c.IsActive);

            if (contact == null || contact.Company == null)
                return NotFound(new { message = "Compte contact ou société introuvable." });

            var projects = await _db.Projets
                .AsNoTracking()
                .Where(p => p.ClientId == contact.CompanyId)
                .Include(p => p.Phases)
                    .ThenInclude(ph => ph.Taches)
                        .ThenInclude(t => t.SousTaches)
                .OrderByDescending(p => p.SyncedAt)
                .ToListAsync();

            var contracts = await _db.Contracts
                .AsNoTracking()
                .Where(c => c.CompanyId == contact.CompanyId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();

            var projectDtos = projects.Select(project =>
            {
                var phases = project.Phases
                    .OrderBy(ph => ph.Ordre)
                    .Select(phase => new ClientPortalPhaseDto
                    {
                        Id = phase.Id,
                        TypePhase = phase.TypePhase,
                        Ordre = phase.Ordre,
                        Taches = phase.Taches
                            .OrderBy(t => t.Ordre)
                            .Select(task => new ClientPortalTaskDto
                            {
                                Id = task.Id,
                                Titre = task.Titre,
                                Statut = task.Statut,
                                Ordre = task.Ordre,
                                ResponsableNom = task.ResponsableNom,
                                SousTaches = task.SousTaches
                                    .OrderBy(st => st.Ordre)
                                    .Select(subTask => new ClientPortalSubTaskDto
                                    {
                                        Id = subTask.Id,
                                        Titre = subTask.Titre,
                                        Statut = subTask.Statut,
                                        DureeEstimeeHeures = subTask.DureeEstimeeHeures,
                                        ResponsableNom = subTask.ResponsableNom,
                                        Ordre = subTask.Ordre,
                                    })
                                    .ToList(),
                            })
                            .ToList(),
                    })
                    .ToList();

                var tasks = phases.SelectMany(phase => phase.Taches).ToList();
                var subTasks = tasks.SelectMany(task => task.SousTaches).ToList();

                return new ClientPortalProjectDto
                {
                    Id = project.Id,
                    Nom = project.Nom,
                    Statut = project.Statut,
                    DateDebut = project.DateDebut,
                    DateFinPrevue = project.DateFinPrevue,
                    BudgetEstime = project.BudgetEstime,
                    BudgetReel = project.BudgetReel,
                    TypeProjet = project.TypeProjet,
                    Phases = phases,
                    TotalTaches = tasks.Count,
                    TachesTerminees = tasks.Count(t => IsDone(t.Statut)),
                    TotalSousTaches = subTasks.Count,
                    SousTachesTerminees = subTasks.Count(st => IsDone(st.Statut)),
                };
            }).ToList();

            var totalBudgetEstime = projectDtos.Sum(p => p.BudgetEstime);
            var totalBudgetReel = projectDtos.Sum(p => p.BudgetReel ?? 0m);
            var totalTasks = projectDtos.Sum(p => p.TotalTaches);
            var totalDoneTasks = projectDtos.Sum(p => p.TachesTerminees);

            return Ok(new ClientPortalDashboardDto
            {
                Contact = new ClientPortalContactDto
                {
                    Id = contact.Id,
                    Nom = contact.Nom,
                    Prenom = contact.Prenom,
                    Email = contact.Email,
                    Poste = contact.Poste,
                    Login = contact.Login,
                    LastLogin = contact.LastLogin,
                },
                Company = new ClientPortalCompanyDto
                {
                    Id = contact.Company.Id,
                    RaisonSociale = contact.Company.RaisonSociale,
                    Secteur = contact.Company.Secteur,
                    Adresse = contact.Company.Adresse,
                    Ville = contact.Company.Ville,
                    Pays = contact.Company.Pays,
                    EmailPrincipal = contact.Company.EmailPrincipal,
                    TelephonePrincipal = contact.Company.TelephonePrincipal,
                    Logo = contact.Company.Logo,
                    Statut = contact.Company.Statut,
                },
                Projects = projectDtos,
                Contracts = contracts.Select(contract => new ClientPortalContractDto
                {
                    Id = contract.Id,
                    Reference = contract.Reference,
                    Status = contract.Status,
                    Amount = contract.Amount,
                    DateStart = contract.DateStart,
                    DateEnd = contract.DateEnd,
                    ProjectId = contract.ProjectId,
                    FilePath = contract.FilePath,
                }).ToList(),
                Summary = new ClientPortalSummaryDto
                {
                    ProjectsCount = projectDtos.Count,
                    ActiveProjectsCount = projectDtos.Count(p => !IsDone(p.Statut)),
                    ContractsCount = contracts.Count,
                    BudgetEstimeTotal = totalBudgetEstime,
                    BudgetReelTotal = totalBudgetReel,
                    TotalTasks = totalTasks,
                    TasksDone = totalDoneTasks,
                }
            });
        }

        private async Task<int?> GetContactIdAsync(CancellationToken ct = default)
        {
            // Ancien systeme JWT local : le sub etait directement l'Id entier du contact.
            var raw = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");
            if (int.TryParse(raw, out var legacyId))
                return legacyId;

            // Authentik : le sub est un UUID -> on resout le contact par son email.
            var email = User.FindFirstValue(ClaimTypes.Email)
                ?? User.FindFirstValue("email")
                ?? User.FindFirstValue("preferred_username");
            if (string.IsNullOrWhiteSpace(email))
                return null;

            return await _db.Contacts.AsNoTracking()
                .Where(c => c.Email == email && c.IsActive)
                .Select(c => (int?)c.Id)
                .FirstOrDefaultAsync(ct);
        }

        private static bool IsDone(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return false;

            var normalized = status.Trim().ToLowerInvariant();
            return normalized is "done" or "terminee" or "terminée" or "closed" or "completed" or "cloturee" or "clôturée";
        }

        public class ClientPortalDashboardDto
        {
            public ClientPortalContactDto Contact { get; set; } = new();
            public ClientPortalCompanyDto Company { get; set; } = new();
            public List<ClientPortalProjectDto> Projects { get; set; } = new();
            public List<ClientPortalContractDto> Contracts { get; set; } = new();
            public ClientPortalSummaryDto Summary { get; set; } = new();
        }

        public class ClientPortalContactDto
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Prenom { get; set; } = string.Empty;
            public string Email { get; set; } = string.Empty;
            public string? Poste { get; set; }
            public string Login { get; set; } = string.Empty;
            public DateTime? LastLogin { get; set; }
        }

        public class ClientPortalCompanyDto
        {
            public int Id { get; set; }
            public string RaisonSociale { get; set; } = string.Empty;
            public string? Secteur { get; set; }
            public string? Adresse { get; set; }
            public string? Ville { get; set; }
            public string Pays { get; set; } = string.Empty;
            public string? EmailPrincipal { get; set; }
            public string? TelephonePrincipal { get; set; }
            public string? Logo { get; set; }
            public string Statut { get; set; } = string.Empty;
        }

        public class ClientPortalProjectDto
        {
            public int Id { get; set; }
            public string Nom { get; set; } = string.Empty;
            public string Statut { get; set; } = string.Empty;
            public DateTime DateDebut { get; set; }
            public DateTime DateFinPrevue { get; set; }
            public string TypeProjet { get; set; } = string.Empty;
            public decimal BudgetEstime { get; set; }
            public decimal? BudgetReel { get; set; }
            public int TotalTaches { get; set; }
            public int TachesTerminees { get; set; }
            public int TotalSousTaches { get; set; }
            public int SousTachesTerminees { get; set; }
            public List<ClientPortalPhaseDto> Phases { get; set; } = new();
        }

        public class ClientPortalPhaseDto
        {
            public int Id { get; set; }
            public string TypePhase { get; set; } = string.Empty;
            public int Ordre { get; set; }
            public List<ClientPortalTaskDto> Taches { get; set; } = new();
        }

        public class ClientPortalTaskDto
        {
            public int Id { get; set; }
            public string Titre { get; set; } = string.Empty;
            public string Statut { get; set; } = string.Empty;
            public int Ordre { get; set; }
            public string? ResponsableNom { get; set; }
            public List<ClientPortalSubTaskDto> SousTaches { get; set; } = new();
        }

        public class ClientPortalSubTaskDto
        {
            public int Id { get; set; }
            public string Titre { get; set; } = string.Empty;
            public string Statut { get; set; } = string.Empty;
            public decimal DureeEstimeeHeures { get; set; }
            public string? ResponsableNom { get; set; }
            public int Ordre { get; set; }
        }

        public class ClientPortalContractDto
        {
            public int Id { get; set; }
            public string Reference { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public decimal Amount { get; set; }
            public DateTime? DateStart { get; set; }
            public DateTime? DateEnd { get; set; }
            public int? ProjectId { get; set; }
            public string? FilePath { get; set; }
        }

        public class ClientPortalSummaryDto
        {
            public int ProjectsCount { get; set; }
            public int ActiveProjectsCount { get; set; }
            public int ContractsCount { get; set; }
            public decimal BudgetEstimeTotal { get; set; }
            public decimal BudgetReelTotal { get; set; }
            public int TotalTasks { get; set; }
            public int TasksDone { get; set; }
        }
    }
}