using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<CompaniesController> _logger;

        public CompaniesController(CrmDbContext db, ILogger<CompaniesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search)
        {
            try
            {
                var query = _db.Companies.AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(c => c.RaisonSociale.Contains(search) || (c.EmailPrincipal != null && c.EmailPrincipal.Contains(search)));

                if (page.HasValue && pageSize.HasValue)
                {
                    var totalCount = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(c => c.CreatedAt)
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToListAsync();

                    return Ok(new PagedResult<Company>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = page.Value,
                        PageSize = pageSize.Value
                    });
                }

                var companies = await query.OrderByDescending(c => c.CreatedAt).ToListAsync();
                return Ok(companies);
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
            var company = await _db.Companies
                .Include(c => c.Contacts)

                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult<Company>> Create(Company company)
        {
            NormalizeAssignment(company);
            company.CreatedAt = DateTime.UtcNow;
            company.UpdatedAt = DateTime.UtcNow;

            _db.Companies.Add(company);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = company.Id }, company);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Company>> Update(int id, Company updatedCompany)
        {
            var existing = await _db.Companies.FindAsync(id);
            if (existing == null)
                return NotFound();

            NormalizeAssignment(updatedCompany);

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
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Companies.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Companies.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
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
    }
}
