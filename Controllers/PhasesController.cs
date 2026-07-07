using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class PhasesController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly INotificationService _notifications;
        private readonly IWebHostEnvironment _env;

        public PhasesController(CrmDbContext db, INotificationService notifications, IWebHostEnvironment env)
        {
            _db = db;
            _notifications = notifications;
            _env = env;
        }

        private static string PhaseLabel(string type) => type switch
        {
            "meeting" => "Réunion",
            "study" => "Étude & CDC",
            "offer" => "Offre",
            "proposition" => "Proposition",
            "contract" => "Contrat",
            _ => type,
        };

        /// <summary>
        /// Définit le set de phases pour une opportunité selon son type.
        /// Helpdesk : proposition → contract.
        /// Sinon    : meeting → study → offer → contract.
        /// </summary>
        private static IEnumerable<(string Type, int Order, string DefaultStatus)> PhaseSpecFor(string? typeProjet)
        {
            var isHelpdesk = string.Equals(typeProjet, "helpdesk", StringComparison.OrdinalIgnoreCase);
            if (isHelpdesk)
            {
                return new[]
                {
                    ("proposition", 0, "not_sent"),
                    ("contract",    1, "pending"),
                };
            }

            return new[]
            {
                ("meeting",  0, "pending"),
                ("study",    1, "pending"),
                ("offer",    2, "not_sent"),
                ("contract", 3, "pending"),
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Phase>>> GetAll()
        {
            var phases = await _db.Phases.AsNoTracking().ToListAsync();
            return Ok(phases);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Phase>> GetById(int id)
        {
            var phase = await _db.Phases.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

            if (phase == null)
                return NotFound();

            return Ok(phase);
        }

        [HttpGet("by-opportunity/{opportunityId}")]
        public async Task<ActionResult<IEnumerable<Phase>>> GetByOpportunity(int opportunityId)
        {
            var phases = await _db.Phases
                .Where(p => p.OpportunityId == opportunityId)
                .Include(p => p.Meetings)
                .OrderBy(p => p.Order)
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

            var spec = PhaseSpecFor(opportunity.TypeProjet);
            var now = DateTime.UtcNow;
            var newPhases = spec.Select(s => new Phase
            {
                OpportunityId = opportunityId,
                Type = s.Type,
                Order = s.Order,
                Status = s.DefaultStatus,
                CreatedAt = now,
                UpdatedAt = now,
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
            existing.AgentResponsableId = updatedPhase.AgentResponsableId;
            existing.DueDate = updatedPhase.DueDate;
            existing.Progress = updatedPhase.Progress;
            existing.Validated = updatedPhase.Validated;
            existing.Montant = updatedPhase.Montant;
            existing.DateEnvoi = updatedPhase.DateEnvoi;
            existing.DateValidite = updatedPhase.DateValidite;
            existing.FeedbackClient = updatedPhase.FeedbackClient;
            existing.DocumentPath = updatedPhase.DocumentPath;
            existing.DocumentName = updatedPhase.DocumentName;
            existing.DocumentContentType = updatedPhase.DocumentContentType;
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

            var opp = await _db.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == existing.OpportunityId);
            await _notifications.NotifyAllAsync(
                "phase_completed",
                $"Phase {PhaseLabel(existing.Type)} terminée",
                opp != null ? $"Opportunité « {opp.Titre} »." : $"Phase #{existing.Id} marquée terminée.",
                $"/crm/pipeline?opp={existing.OpportunityId}");

            return Ok(existing);
        }

        [HttpPut("{id}/change-status")]
        public async Task<ActionResult<object>> ChangeStatus(int id, [FromQuery] string status)
        {
            var existing = await _db.Phases.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Status = status;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Retourne la phase + une éventuelle suggestion de pipelineStage
            var opp = await _db.Opportunities.AsNoTracking().FirstOrDefaultAsync(o => o.Id == existing.OpportunityId);
            var suggestion = SuggestPipelineStage(existing, opp);

            return Ok(new
            {
                phase = existing,
                suggestedPipelineStage = suggestion,
                currentPipelineStage = opp?.PipelineStage,
            });
        }

        /// <summary>
        /// Suggère le stage suivant du pipeline en fonction du statut de la phase.
        /// Retourne null si aucune transition à proposer.
        /// La règle ne propose que des avancées (jamais de recul).
        /// </summary>
        private static string? SuggestPipelineStage(Phase phase, Opportunity? opp)
        {
            if (opp == null) return null;

            var current = (opp.PipelineStage ?? "prospection").ToLowerInvariant();
            if (current == "gagnee" || current == "perdue") return null;

            string? target = (phase.Type, phase.Status) switch
            {
                ("meeting", "completed")    => "qualification",
                ("study", "completed")      when phase.Validated => "qualification",
                ("offer", "sent")           => "negociation",
                ("offer", "accepted")       => "negociation",
                ("offer", "refused")        => "perdue",
                ("proposition", "sent")     => "negociation",
                ("proposition", "accepted") => "negociation",
                ("proposition", "refused")  => "perdue",
                ("contract", "signed")      => "gagnee",
                _ => null,
            };

            if (target == null) return null;
            if (StageRank(target) <= StageRank(current)) return null;
            return target;
        }

        private static int StageRank(string stage) => stage switch
        {
            "prospection"   => 0,
            "qualification" => 1,
            "negociation"   => 2,
            "gagnee"        => 3,
            "perdue"        => 3,
            _ => -1,
        };

        // ─── Meetings (multi-réunions par phase meeting) ───

        [HttpGet("{phaseId}/meetings")]
        public async Task<ActionResult<IEnumerable<Meeting>>> GetMeetings(int phaseId)
        {
            var meetings = await _db.Meetings
                .Where(m => m.PhaseId == phaseId)
                .OrderBy(m => m.Date).ThenBy(m => m.Time)
                .ToListAsync();
            return Ok(meetings);
        }

        [HttpPost("{phaseId}/meetings")]
        public async Task<ActionResult<Meeting>> AddMeeting(int phaseId, [FromBody] Meeting meeting)
        {
            var phase = await _db.Phases.FindAsync(phaseId);
            if (phase == null) return NotFound(new { message = "Phase introuvable." });
            if (phase.Type != "meeting") return BadRequest(new { message = "Cette phase n'est pas de type 'meeting'." });

            meeting.Id = 0;
            meeting.PhaseId = phaseId;
            meeting.CreatedAt = DateTime.UtcNow;
            meeting.UpdatedAt = DateTime.UtcNow;
            _db.Meetings.Add(meeting);
            await _db.SaveChangesAsync();
            return Ok(meeting);
        }

        [HttpPut("meetings/{meetingId}")]
        public async Task<ActionResult<Meeting>> UpdateMeeting(int meetingId, [FromBody] Meeting updated)
        {
            var existing = await _db.Meetings.FindAsync(meetingId);
            if (existing == null) return NotFound();

            existing.Date = updated.Date;
            existing.Time = updated.Time;
            existing.Title = updated.Title;
            existing.Lieu = updated.Lieu;
            existing.Notes = updated.Notes;
            existing.Participants = updated.Participants;
            existing.Done = updated.Done;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(existing);
        }

        [HttpDelete("meetings/{meetingId}")]
        public async Task<ActionResult> DeleteMeeting(int meetingId)
        {
            var existing = await _db.Meetings.FindAsync(meetingId);
            if (existing == null) return NotFound();
            _db.Meetings.Remove(existing);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // ─── Upload de document de phase (proposition contrat, document d'étude) ───

        [HttpPost("{phaseId}/upload-document")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Phase>> UploadPhaseDocument(int phaseId, [FromForm] PhaseDocumentUploadDto dto)
        {
            var phase = await _db.Phases.FindAsync(phaseId);
            if (phase == null) return NotFound(new { message = "Phase introuvable." });

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Aucun fichier fourni." });

            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            var allowed = new[] { ".pdf", ".doc", ".docx" };
            if (!allowed.Contains(extension))
                return BadRequest(new { message = "Type de fichier non autorisé (PDF/DOC/DOCX)." });

            if (dto.File.Length > 20 * 1024 * 1024)
                return BadRequest(new { message = "Fichier trop volumineux (max 20 Mo)." });

            phase.DocumentPath = await DocumentStorage.SaveAsync(_db, dto.File);
            phase.DocumentName = dto.File.FileName;
            phase.DocumentContentType = dto.File.ContentType;
            phase.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok(phase);
        }

        public class PhaseDocumentUploadDto
        {
            public IFormFile? File { get; set; }
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
