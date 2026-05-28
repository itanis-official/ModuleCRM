using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ProjetsController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly EquipeApiService _equipeApiService;

        public ProjetsController(CrmDbContext db, EquipeApiService equipeApiService)
        {
            _db = db;
            _equipeApiService = equipeApiService;
        }

        private string NormalizeRole(string? rawRole)
        {
            if (string.IsNullOrWhiteSpace(rawRole)) return "agent";
            var token = rawRole.Normalize(System.Text.NormalizationForm.FormD).ToLowerInvariant();
            token = System.Text.RegularExpressions.Regex.Replace(token, "[^a-z0-9]", "");

            if (token == "superadmin" || token == "admin" || token == "administrateur" || token.Contains("superadmin")) return "super_admin";
            if (token == "rh" || token == "ressourceshumaines") return "rh";
            if (token == "chefprojet" || token == "chefdeprojet" || token == "manager" || token == "lead") return "chef_projet";
            if (token == "agentcommercial" || token == "commercial") return "agent_commercial";
            if (token == "contact" || token == "client") return "contact";
            
            return "agent";
        }

        [HttpGet("by-company/{companyId}")]
        public async Task<ActionResult<IEnumerable<ProjetCrmListItemDto>>> GetByCompany(int companyId)
        {
            var userRole = User.GetItanisRole();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            // Access check
            if (userRole == "super_admin" || userRole == "agent_commercial" || userRole == "rh")
            {
                // Full access
            }
            else if (userRole == "agent")
            {
                var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId);
                if (company != null && company.AgentResponsableId != userId && (company.AgentResponsableId != null || company.EquipeResponsableId != null))
                {
                    return Forbid();
                }
            }
            else if (userRole == "chef_projet")
            {
                var company = await _db.Companies.AsNoTracking().FirstOrDefaultAsync(c => c.Id == companyId);
                if (company != null && company.AgentResponsableId != userId)
                {
                    bool isUnassigned = company.AgentResponsableId == null && company.EquipeResponsableId == null;
                    if (!isUnassigned)
                    {
                        var allEquipes = await _equipeApiService.GetAllAsync();
                        var myEquipeIds = allEquipes.Where(e => e.ChefProjet?.Id == userId).Select(e => e.Id).ToList();
                        bool isOwnEquipe = company.AffectationType == "equipe" && company.EquipeResponsableId != null && myEquipeIds.Contains(company.EquipeResponsableId.Value);
                        
                        if (!isOwnEquipe) return Forbid();
                    }
                }
            }

            var projets = await _db.Projets
                .AsNoTracking()
                .Where(p => p.ClientId == companyId)
                .OrderByDescending(p => p.SyncedAt)
                .Select(p => new ProjetCrmListItemDto
                {
                    Id = p.Id,
                    OpportuniteIdOrigine = p.OpportuniteIdOrigine,
                    Nom = p.Nom,
                    Statut = p.Statut,
                    DateDebut = p.DateDebut,
                    DateFinPrevue = p.DateFinPrevue,
                    ClientRaisonSociale = p.ClientRaisonSociale,
                    BudgetEstime = p.BudgetEstime,
                    BudgetReel = p.BudgetReel,
                })
                .ToListAsync();
            return Ok(projets);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjetCrmListItemDto>>> GetAll()
        {
            var userRole = User.GetItanisRole();
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub") ?? User.Claims.FirstOrDefault(c => c.Type == "sub" || c.Type == ClaimTypes.NameIdentifier)?.Value;
            int.TryParse(userIdStr, out var userId);

            var query = _db.Projets.AsQueryable();

            if (userRole == "agent")
            {
                query = query.Where(p => p.Company != null && (
                    p.Company.AgentResponsableId == userId ||
                    (p.Company.AgentResponsableId == null && p.Company.EquipeResponsableId == null)
                ));
            }
            else if (userRole == "chef_projet")
            {
                var allEquipes = await _equipeApiService.GetAllAsync();
                var myEquipeIds = allEquipes
                    .Where(e => e.ChefProjet?.Id == userId)
                    .Select(e => e.Id)
                    .ToList();

                query = query.Where(p => p.Company != null && (
                    (p.Company.AffectationType == "equipe" && p.Company.EquipeResponsableId != null && myEquipeIds.Contains(p.Company.EquipeResponsableId.Value)) ||
                    (p.Company.AgentResponsableId == userId) ||
                    (p.Company.AgentResponsableId == null && p.Company.EquipeResponsableId == null)
                ));
            }
            else if (userRole != "super_admin" && userRole != "agent_commercial" && userRole != "rh")
            {
                query = query.Where(p => p.Company != null && (
                    p.Company.AgentResponsableId == userId ||
                    (p.Company.AgentResponsableId == null && p.Company.EquipeResponsableId == null)
                ));
            }
            
            var projets = await query
                .AsNoTracking()
                .OrderByDescending(p => p.SyncedAt)
                .Select(p => new ProjetCrmListItemDto
                {
                    Id = p.Id,
                    OpportuniteIdOrigine = p.OpportuniteIdOrigine,
                    Nom = p.Nom,
                    Statut = p.Statut,
                    DateDebut = p.DateDebut,
                    DateFinPrevue = p.DateFinPrevue,
                    ClientRaisonSociale = p.ClientRaisonSociale,
                    BudgetEstime = p.BudgetEstime,
                    BudgetReel = p.BudgetReel,
                })
                .ToListAsync();
            return Ok(projets);
        }

        [HttpGet("by-opportunite/{opportuniteId}")]
        public async Task<ActionResult<ProjetCrmDetailDto>> GetByOpportunite(int opportuniteId)
        {
            var p = await _db.Projets
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.OpportuniteIdOrigine == opportuniteId);

            if (p == null)
                return NotFound(new { message = "Aucun projet trouvé pour cette opportunité." });

            return Ok(new ProjetCrmDetailDto
            {
                Id = p.Id,
                Nom = p.Nom,
                Statut = p.Statut,
                DateDebut = p.DateDebut,
                DateFinPrevue = p.DateFinPrevue,
                Avancement = p.Statut,
                Client = new ClientCrmDto
                {
                    Id = p.ClientId,
                    RaisonSociale = p.ClientRaisonSociale,
                },
            });
        }

        [HttpGet("{id}/phases")]
        public async Task<ActionResult<IEnumerable<ProjetPhaseDto>>> GetPhases(int id)
        {
            var exists = await _db.Projets.AnyAsync(p => p.Id == id);
            if (!exists)
                return NotFound(new { message = "Projet introuvable." });

            var phases = await _db.ProjetPhases
                .AsNoTracking()
                .Where(ph => ph.ProjetId == id)
                .OrderBy(ph => ph.Ordre)
                .Select(ph => new ProjetPhaseDto
                {
                    Id = ph.Id,
                    TypePhase = ph.TypePhase,
                    Ordre = ph.Ordre,
                    Taches = ph.Taches
                        .OrderBy(t => t.Ordre)
                        .Select(t => new ProjetTacheDto
                        {
                            Id = t.Id,
                            Titre = t.Titre,
                            Statut = t.Statut,
                            Ordre = t.Ordre,
                            ResponsableNom = t.ResponsableNom,
                            SousTaches = t.SousTaches
                                .OrderBy(s => s.Ordre)
                                .Select(s => new ProjetSousTacheDto
                                {
                                    Id = s.Id,
                                    Titre = s.Titre,
                                    Statut = s.Statut,
                                    DureeEstimeeHeures = s.DureeEstimeeHeures,
                                    ResponsableNom = s.ResponsableNom,
                                    Ordre = s.Ordre,
                                })
                                .ToList(),
                        })
                        .ToList(),
                })
                .ToListAsync();

            return Ok(phases);
        }

        [HttpGet("by-opportunite/{opportuniteId}/phases")]
        public async Task<ActionResult<IEnumerable<ProjetPhaseDto>>> GetPhasesByOpportunite(int opportuniteId)
        {
            var projet = await _db.Projets
                .AsNoTracking()
                .FirstOrDefaultAsync(p => p.OpportuniteIdOrigine == opportuniteId);

            if (projet == null)
                return NotFound(new { message = "Aucun projet trouvé pour cette opportunité." });

            return await GetPhases(projet.Id);
        }
    }
}
