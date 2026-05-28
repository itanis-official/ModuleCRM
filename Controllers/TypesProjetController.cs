using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;
using MassTransit;
using ITANIS.SharedEvents;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TypesProjetController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<TypesProjetController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;

        public TypesProjetController(
            CrmDbContext db,
            ILogger<TypesProjetController> logger,
            IPublishEndpoint publishEndpoint)
        {
            _db = db;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<ActionResult> GetAll([FromQuery] bool includeInactive = false)
        {
            var query = _db.TypesProjet.AsNoTracking();
            if (!includeInactive)
                query = query.Where(t => t.IsActive);

            var items = await query
                .OrderBy(t => t.Ordre)
                .ThenBy(t => t.Label)
                .Select(t => new
                {
                    t.Id,
                    t.TypeProjetGuid,
                    t.Value,
                    t.Label,
                    t.IsActive,
                    t.Ordre,
                })
                .ToListAsync();

            return Ok(items);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetById(int id)
        {
            var item = await _db.TypesProjet.FindAsync(id);
            return item == null ? NotFound() : Ok(item);
        }

        public class UpsertTypeProjetRequest
        {
            public string Value { get; set; } = string.Empty;
            public string Label { get; set; } = string.Empty;
            public bool IsActive { get; set; } = true;
            public int Ordre { get; set; }
        }

        [HttpPost]
        public async Task<ActionResult> Create([FromBody] UpsertTypeProjetRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Value) || string.IsNullOrWhiteSpace(request.Label))
                return BadRequest(new { message = "Value et Label sont requis." });

            var exists = await _db.TypesProjet.AnyAsync(t => t.Value == request.Value);
            if (exists)
                return Conflict(new { message = $"Un type avec la valeur '{request.Value}' existe déjà." });

            var type = new TypeProjet
            {
                Value = request.Value.Trim(),
                Label = request.Label.Trim(),
                IsActive = request.IsActive,
                Ordre = request.Ordre,
            };

            _db.TypesProjet.Add(type);
            await _db.SaveChangesAsync();

            await PublishSync(type, SyncAction.Created);

            return CreatedAtAction(nameof(GetById), new { id = type.Id }, type);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult> Update(int id, [FromBody] UpsertTypeProjetRequest request)
        {
            var existing = await _db.TypesProjet.FindAsync(id);
            if (existing == null)
                return NotFound();

            if (!string.Equals(existing.Value, request.Value, StringComparison.Ordinal))
            {
                var conflict = await _db.TypesProjet.AnyAsync(t => t.Value == request.Value && t.Id != id);
                if (conflict)
                    return Conflict(new { message = $"Un autre type utilise déjà la valeur '{request.Value}'." });
                existing.Value = request.Value.Trim();
            }

            existing.Label = request.Label.Trim();
            existing.IsActive = request.IsActive;
            existing.Ordre = request.Ordre;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await PublishSync(existing, SyncAction.Updated);

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.TypesProjet.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.TypesProjet.Remove(existing);
            await _db.SaveChangesAsync();

            await PublishSync(existing, SyncAction.Deleted);

            return NoContent();
        }

        private async Task PublishSync(TypeProjet type, SyncAction action)
        {
            try
            {
                await _publishEndpoint.Publish(new TypeProjetSyncEvent
                {
                    TypeProjetGuid = type.TypeProjetGuid,
                    Action = action,
                    Id = type.Id,
                    Value = type.Value,
                    Label = type.Label,
                    IsActive = type.IsActive,
                    Ordre = type.Ordre,
                    ChangedAt = DateTime.UtcNow,
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Echec publication TypeProjetSyncEvent pour Id={Id} Action={Action}",
                    type.Id, action);
            }
        }
    }
}
