using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunitiesController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public OpportunitiesController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Opportunity>>> GetAll()
        {
            var opportunities = await _db.Opportunities
                .Include(o => o.Company)
                .Include(o => o.ProjectParent)
                .Include(o => o.AgentCommercial)
                .ToListAsync();
            return Ok(opportunities);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Opportunity>> GetById(int id)
        {
            var opportunity = await _db.Opportunities
                .Include(o => o.Company)
                .Include(o => o.ProjectParent)
                .Include(o => o.AgentCommercial)
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
            existing.AgentCommercialId = updatedOpportunity.AgentCommercialId;
            existing.AgentCdcId = updatedOpportunity.AgentCdcId;
            existing.EcheanceCdc = updatedOpportunity.EcheanceCdc;
            existing.CdcFilePath = updatedOpportunity.CdcFilePath;
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
