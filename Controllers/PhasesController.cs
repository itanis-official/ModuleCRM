using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhasesController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public PhasesController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Phase>>> GetAll()
        {
            var phases = await _db.Phases
                .Include(p => p.Opportunity)
                .ToListAsync();
            return Ok(phases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Phase>> GetById(int id)
        {
            var phase = await _db.Phases
                .Include(p => p.Opportunity)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (phase == null)
                return NotFound();

            return Ok(phase);
        }

        [HttpGet("ByOpportunity/{opportunityId}")]
        public async Task<ActionResult<IEnumerable<Phase>>> GetByOpportunity(int opportunityId)
        {
            var phases = await _db.Phases
                .Where(p => p.OpportunityId == opportunityId)
                .OrderBy(p => p.Type == "meeting" ? 0 : p.Type == "study" ? 1 : p.Type == "offer" ? 2 : 3)
                .ToListAsync();

            return Ok(phases);
        }

        [HttpPost]
        public async Task<ActionResult<Phase>> Create(Phase phase)
        {
            phase.CreatedAt = DateTime.UtcNow;
            phase.UpdatedAt = DateTime.UtcNow;

            _db.Phases.Add(phase);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = phase.Id }, phase);
        }

        [HttpPost("InitForOpportunity/{opportunityId}")]
        public async Task<ActionResult<IEnumerable<Phase>>> InitForOpportunity(int opportunityId)
        {
            var opportunity = await _db.Opportunities.FindAsync(opportunityId);
            if (opportunity == null)
                return NotFound();

            var existingPhases = await _db.Phases
                .Where(p => p.OpportunityId == opportunityId)
                .ToListAsync();

            if (existingPhases.Any())
                return BadRequest("Les phases existent deja pour cette opportunite.");

            var phaseTypes = new[] { "meeting", "study", "offer", "contract" };
            var newPhases = phaseTypes.Select(type => new Phase
            {
                OpportunityId = opportunityId,
                Type = type,
                Status = type == "offer" ? "not_sent" : "pending",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }).ToList();

            _db.Phases.AddRange(newPhases);
            await _db.SaveChangesAsync();

            return Ok(newPhases);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Phase>> Update(int id, Phase updatedPhase)
        {
            var existing = await _db.Phases.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Status = updatedPhase.Status;
            existing.Notes = updatedPhase.Notes;
            existing.MeetingDate = updatedPhase.MeetingDate;
            existing.MeetingTime = updatedPhase.MeetingTime;
            existing.AgentEtudeId = updatedPhase.AgentEtudeId;
            existing.DueDate = updatedPhase.DueDate;
            existing.Progress = updatedPhase.Progress;
            existing.Validated = updatedPhase.Validated;
            existing.Montant = updatedPhase.Montant;
            existing.DateEnvoi = updatedPhase.DateEnvoi;
            existing.DateValidite = updatedPhase.DateValidite;
            existing.FeedbackClient = updatedPhase.FeedbackClient;
            existing.Reference = updatedPhase.Reference;
            existing.DateSignature = updatedPhase.DateSignature;
            existing.Signed = updatedPhase.Signed;
            existing.DocumentPath = updatedPhase.DocumentPath;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpPut("{id}/complete")]
        public async Task<ActionResult<Phase>> MarkComplete(int id)
        {
            var existing = await _db.Phases.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Status = "completed";
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpPut("{id}/change-status")]
        public async Task<ActionResult<Phase>> ChangeStatus(int id, [FromQuery] string status)
        {
            var existing = await _db.Phases.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Phases.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Phases.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
