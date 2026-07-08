using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Models;
using ModuleCRM.Services;
using ModuleCRM.Services;
using MassTransit;
using ITANIS.SharedEvents;
using Microsoft.Extensions.Logging;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OpportunitiesController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<OpportunitiesController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly INotificationService _notifications;

        // Transitions autorisées. Volontairement souple pour permettre les sauts
        // (ex: helpdesk peut passer directement de prospection à gagnée si contrat signé).
        // Les stages terminaux (gagnée/perdue) restent verrouillés pour éviter les rollbacks accidentels.
        private static readonly Dictionary<string, HashSet<string>> ValidTransitions = new()
        {
            ["prospection"]   = new() { "qualification", "negociation", "gagnee", "perdue" },
            ["qualification"] = new() { "prospection", "negociation", "gagnee", "perdue" },
            ["negociation"]   = new() { "prospection", "qualification", "gagnee", "perdue" },
            ["gagnee"]        = new(),
            ["perdue"]        = new() { "prospection" }, // rouvrir une opportunité perdue
        };

        public OpportunitiesController(CrmDbContext db, IWebHostEnvironment env, ILogger<OpportunitiesController> logger, IPublishEndpoint publishEndpoint, INotificationService notifications)
        {
            _db = db;
            _env = env;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _notifications = notifications;
        }

        [HttpPost("upload-cdc")]
        [RequestSizeLimit(25_000_000)]
        public async Task<ActionResult> UploadCdc(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { message = "Aucun fichier fourni." });

            var filePath = await DocumentStorage.SaveAsync(_db, file);
            return Ok(new
            {
                fileName = file.FileName,
                contentType = file.ContentType,
                filePath,
            });
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

            // Auto-initialiser les phases selon le type d'opportunité
            // Helpdesk : proposition → contract
            // Sinon    : meeting → study → offer → contract
            var isHelpdesk = string.Equals(opportunity.TypeProjet, "helpdesk", StringComparison.OrdinalIgnoreCase);
            var phaseSpec = isHelpdesk
                ? new[] { ("proposition", 0, "not_sent"), ("contract", 1, "pending") }
                : new[]
                  {
                      ("meeting",  0, "pending"),
                      ("study",    1, "pending"),
                      ("offer",    2, "not_sent"),
                      ("contract", 3, "pending"),
                  };

            var now = DateTime.UtcNow;
            foreach (var (type, order, status) in phaseSpec)
            {
                _db.Phases.Add(new Phase
                {
                    OpportunityId = opportunity.Id,
                    Type = type,
                    Order = order,
                    Status = status,
                    CreatedAt = now,
                    UpdatedAt = now,
                });
            }
            await _db.SaveChangesAsync();

            await _notifications.NotifyAllAsync(
                "opportunity_created",
                "Nouvelle opportunité",
                $"« {opportunity.Titre} » vient d'être créée.",
                $"/crm/pipeline?opp={opportunity.Id}");

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

            var currentStage = (existing.PipelineStage ?? "prospection").ToLowerInvariant();
            var targetStage = stage.ToLowerInvariant();

            // No-op si même stage (cas drag-drop qui renvoie la colonne d'origine)
            if (currentStage == targetStage)
                return Ok(existing);

            if (!ValidTransitions.TryGetValue(currentStage, out var allowed) || !allowed.Contains(targetStage))
                return BadRequest(new { message = $"Transition invalide: '{currentStage}' → '{targetStage}'." });

            existing.PipelineStage = targetStage;
            if (targetStage == "perdue" && !string.IsNullOrEmpty(raisonPerte))
                existing.RaisonPerte = raisonPerte;
            if (targetStage == "gagnee" || targetStage == "perdue")
                existing.DateCloture = DateTime.UtcNow;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            // Opportunite gagnee -> le(s) contrat(s) passe(nt) "signed" (deal conclu).
            if (targetStage == "gagnee")
            {
                await _db.Contracts
                    .Where(c => c.ProjectId == existing.Id)
                    .ExecuteUpdateAsync(s => s.SetProperty(c => c.Status, "signed"));
            }

            var stageLabel = targetStage switch
            {
                "qualification" => "Qualification",
                "negociation" => "Négociation",
                "gagnee" => "Gagnée",
                "perdue" => "Perdue",
                _ => targetStage,
            };
            var notifType = targetStage == "gagnee" ? "opportunity_won"
                          : targetStage == "perdue" ? "opportunity_lost"
                          : "opportunity_stage_changed";
            await _notifications.NotifyAllAsync(
                notifType,
                $"Opportunité {stageLabel.ToLowerInvariant()}",
                $"« {existing.Titre} » est passée à l'étape : {stageLabel}.",
                $"/crm/pipeline?opp={existing.Id}");

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

        [HttpPost("{id}/upload-cdc")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Opportunity>> UploadCdc(int id, [FromForm] OpportunityCdcUploadDto dto)
        {
            var opportunity = await _db.Opportunities.FindAsync(id);
            if (opportunity == null)
                return NotFound(new { message = "Opportunité introuvable." });

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Aucun fichier CDC fourni." });

            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Type de fichier non autorisé. Formats acceptés: PDF, DOCX, DOC." });

            if (dto.File.Length > 20 * 1024 * 1024)
                return BadRequest(new { message = "Le fichier dépasse la taille maximale de 20 Mo." });

            opportunity.CdcFilePath = await DocumentStorage.SaveAsync(_db, dto.File);
            opportunity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _notifications.NotifyAllAsync(
                "cdc_uploaded",
                "Cahier des charges attaché",
                $"Un CDC a été téléversé pour l'opportunité « {opportunity.Titre} ».",
                $"/crm/pipeline?opp={opportunity.Id}");

            return Ok(opportunity);
        }

        public class ConvertToProjectRequest
        {
            public string? NomProjet { get; set; }
            public string? DescriptionProjet { get; set; }
            public DateTime? DateDebutProjet { get; set; }
            public decimal? BudgetConfirme { get; set; }
            public int? EquipeId { get; set; }
        }

        [HttpPost("{id}/convert-to-project")]
        public async Task<ActionResult> ConvertToProject(int id, [FromBody] ConvertToProjectRequest? request = null)
        {
            var opportunity = await _db.Opportunities
                .Include(o => o.Company)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (opportunity == null)
                return NotFound();

            if ((opportunity.PipelineStage ?? "").ToLowerInvariant() != "gagnee")
                return BadRequest(new { message = "Seules les opportunités au stade 'gagnée' peuvent être converties en projet." });

            var alreadyConverted = await _db.Projets
                .AsNoTracking()
                .AnyAsync(p => p.OpportuniteIdOrigine == opportunity.Id);
            if (alreadyConverted)
                return BadRequest(new { message = "Cette opportunité a déjà été convertie en projet." });

            var userIdClaim = User.FindFirst("sub")
                              ?? User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier);
            var convertedBy = userIdClaim?.Value ?? "unknown";

            string? cdcFileName = opportunity.CdcFileName;
            string? cdcContentType = opportunity.CdcContentType;
            byte[]? cdcFileContent = null;

            if (!string.IsNullOrWhiteSpace(opportunity.CdcFilePath)
                && int.TryParse(Path.GetFileName(opportunity.CdcFilePath), out var cdcDocId) && cdcDocId > 0)
            {
                // CdcFilePath = "/documents/{docId}" -> contenu stocke en base.
                var cdcDoc = await _db.DocumentsCrm.FindAsync(cdcDocId);
                if (cdcDoc != null)
                {
                    cdcFileContent = cdcDoc.Donnees;
                    if (string.IsNullOrWhiteSpace(cdcFileName)) cdcFileName = cdcDoc.NomFichier;
                    if (string.IsNullOrWhiteSpace(cdcContentType)) cdcContentType = cdcDoc.TypeContenu;
                }
                else
                {
                    _logger.LogWarning("CDC introuvable en base pour l'opportunité {OpportunityId} (doc {DocId})", opportunity.Id, cdcDocId);
                }
            }

            // Résoudre le GUID de l'équipe via un appel HTTP vers ModuleRH (équipe réside côté RH).
            // Si non trouvé, on publie sans EquipeGuid — Nesrine tombera en fallback sur EquipeIdOrigine.
            Guid? equipeGuid = null;
            if (request?.EquipeId.HasValue == true)
            {
                try
                {
                    var rhBaseUrl = HttpContext.RequestServices
                        .GetRequiredService<IConfiguration>()["ModuleRH:BaseUrl"] ?? "http://localhost:5085";
                    using var httpClient = new HttpClient { BaseAddress = new Uri(rhBaseUrl) };
                    var authHeader = Request.Headers["Authorization"].ToString();
                    if (!string.IsNullOrEmpty(authHeader))
                        httpClient.DefaultRequestHeaders.Add("Authorization", authHeader);

                    var response = await httpClient.GetAsync($"/api/Equipes/{request.EquipeId.Value}");
                    if (response.IsSuccessStatusCode)
                    {
                        var json = await response.Content.ReadAsStringAsync();
                        using var doc = System.Text.Json.JsonDocument.Parse(json);
                        if (doc.RootElement.TryGetProperty("equipeGuid", out var guidProp) && guidProp.TryGetGuid(out var g))
                            equipeGuid = g;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Impossible de résoudre EquipeGuid pour EquipeId={Id}, publication sans GUID.", request.EquipeId);
                }
            }

            // Publier l'événement sur RabbitMQ via MassTransit
            await _publishEndpoint.Publish(new OpportuniteConvertieEvent
            {
                OpportunityId = opportunity.Id,
                Titre = opportunity.Titre,
                Description = opportunity.Description,
                CompanyIdOrigine = opportunity.CompanyId,
                CompanyName = opportunity.Company?.RaisonSociale ?? "",
                ValeurEstimee = opportunity.ValeurEstimee ?? 0m,
                AgentCommercialIdOrigine = opportunity.AgentCommercialId,
                AgentCdcIdOrigine = opportunity.AgentCdcId,
                DateCloturePrevu = opportunity.DateCloturePrevu,
                Type = opportunity.Type,
                SubType = opportunity.SubType,
                TypeProjet = opportunity.TypeProjet,
                CdcFilePath = opportunity.CdcFilePath,
                CdcFileName = cdcFileName,
                CdcContentType = cdcContentType,
                CdcFileContent = cdcFileContent,
                NomProjet = request?.NomProjet,
                DescriptionProjet = request?.DescriptionProjet,
                DateDebutProjet = request?.DateDebutProjet,
                BudgetConfirme = request?.BudgetConfirme,
                EquipeIdOrigine = request?.EquipeId,
                EquipeGuid = equipeGuid,
                ConvertedBy = convertedBy,
                ConvertedAt = DateTime.UtcNow,
            });

            if (request?.EquipeId.HasValue == true)
            {
                opportunity.EquipeId = request.EquipeId;
            }

            opportunity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            _logger.LogInformation(
                "Opportunité {Id} convertie en projet via RabbitMQ (CDC: {CdcFilePath})",
                opportunity.Id, opportunity.CdcFilePath ?? "aucun");

            return Ok(new { message = "Conversion en projet envoyée avec succès.", opportunityId = opportunity.Id });
        }

        private static string GetContentType(string fileName)
        {
            return Path.GetExtension(fileName).ToLowerInvariant() switch
            {
                ".pdf" => "application/pdf",
                ".doc" => "application/msword",
                ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                _ => "application/octet-stream"
            };
        }

        public class OpportunityCdcUploadDto
        {
            public IFormFile? File { get; set; }
        }

        [HttpPost("{id}/upload-proposition-contrat")]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Opportunity>> UploadPropositionContrat(int id, [FromForm] OpportunityCdcUploadDto dto)
        {
            var opportunity = await _db.Opportunities.FindAsync(id);
            if (opportunity == null)
                return NotFound(new { message = "Opportunité introuvable." });

            if (dto.File == null || dto.File.Length == 0)
                return BadRequest(new { message = "Aucun fichier fourni." });

            var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
            if (!allowedExtensions.Contains(extension))
                return BadRequest(new { message = "Type de fichier non autorisé. Formats acceptés: PDF, DOCX, DOC." });

            if (dto.File.Length > 20 * 1024 * 1024)
                return BadRequest(new { message = "Le fichier dépasse la taille maximale de 20 Mo." });

            opportunity.PropositionContratFilePath = await DocumentStorage.SaveAsync(_db, dto.File);
            opportunity.PropositionContratFileName = dto.File.FileName;
            opportunity.PropositionContratContentType = dto.File.ContentType;
            opportunity.UpdatedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            return Ok(opportunity);
        }
    }
}
