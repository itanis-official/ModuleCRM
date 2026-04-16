using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Models;
using Microsoft.Extensions.Logging;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunitiesController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<OpportunitiesController> _logger;

        public OpportunitiesController(CrmDbContext db, ILogger<OpportunitiesController> logger)
        {
            _db = db;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult> GetAll([FromQuery] int? page, [FromQuery] int? pageSize, [FromQuery] string? search)
        {
            try
            {
                var query = _db.Opportunities.AsNoTracking().AsQueryable();

                if (!string.IsNullOrWhiteSpace(search))
                    query = query.Where(o => o.Titre.Contains(search));

                if (page.HasValue && pageSize.HasValue)
                {
                    var totalCount = await query.CountAsync();
                    var items = await query
                        .OrderByDescending(o => o.CreatedAt)
                        .Skip((page.Value - 1) * pageSize.Value)
                        .Take(pageSize.Value)
                        .ToListAsync();

                    return Ok(new PagedResult<Opportunity>
                    {
                        Items = items,
                        TotalCount = totalCount,
                        Page = page.Value,
                        PageSize = pageSize.Value
                    });
                }

                var opportunities = await query.OrderByDescending(o => o.CreatedAt).ToListAsync();
                return Ok(opportunities);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting opportunities");
                return StatusCode(500, new { message = "Une erreur est survenue lors de la récupération des opportunités." });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Opportunity>> GetById(int id)
        {
            var opportunity = await _db.Opportunities
                .AsNoTracking()
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound();

            return Ok(opportunity);
        }

        [HttpPost]
        public async Task<ActionResult<Opportunity>> Create(Opportunity opportunity)
        {
            opportunity.CreatedAt = DateTime.UtcNow;
            opportunity.UpdatedAt = DateTime.UtcNow;

            _db.Opportunities.Add(opportunity);
            await _db.SaveChangesAsync();

            // Auto-initialiser les 4 phases pour la nouvelle opportunité
            var phaseTypes = new[] { "meeting", "study", "offer", "contract" };
            foreach (var type in phaseTypes)
            {
                _db.Phases.Add(new Phase
                {
                    OpportunityId = opportunity.Id,
                    Type = type,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                });
            }
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = opportunity.Id }, opportunity);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Opportunity>> Update(int id, Opportunity updatedOpportunity)
        {
            var existing = await _db.Opportunities.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Titre = updatedOpportunity.Titre;
            existing.Description = updatedOpportunity.Description;
            existing.ValeurEstimee = updatedOpportunity.ValeurEstimee;
            existing.Probabilite = updatedOpportunity.Probabilite;
            existing.PipelineStage = updatedOpportunity.PipelineStage;
            existing.DateCloturePrevu = updatedOpportunity.DateCloturePrevu;
            existing.DateCloture = updatedOpportunity.DateCloture;
            existing.Type = updatedOpportunity.Type;
            existing.SubType = updatedOpportunity.SubType;
            existing.TypeProjet = updatedOpportunity.TypeProjet;
            existing.AgentCommercialId = updatedOpportunity.AgentCommercialId;
            existing.AgentCdcId = updatedOpportunity.AgentCdcId;
            existing.EcheanceCdc = updatedOpportunity.EcheanceCdc;
            existing.CdcFilePath = updatedOpportunity.CdcFilePath;
            existing.RaisonPerte = updatedOpportunity.RaisonPerte;
            existing.Notes = updatedOpportunity.Notes;
            existing.CompanyId = updatedOpportunity.CompanyId;
            existing.ProjectParentId = updatedOpportunity.ProjectParentId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Opportunities.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Opportunities.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpPut("{id}/change-stage")]
        public async Task<ActionResult> ChangeStage(int id, [FromQuery] string stage, [FromQuery] string? raisonPerte = null)
        {
            var existing = await _db.Opportunities.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.PipelineStage = stage;
            if (stage == "perdue" && !string.IsNullOrEmpty(raisonPerte))
                existing.RaisonPerte = raisonPerte;
            if (stage == "gagnee" || stage == "perdue")
                existing.DateCloture = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetByCompany(int companyId)
        {
            var opportunities = await _db.Opportunities
                .Where(o => o.CompanyId == companyId)
                .ToListAsync();

            return Ok(opportunities);
        }
    }
}
