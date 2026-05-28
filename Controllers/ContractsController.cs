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
    public class ContractsController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly IWebHostEnvironment _env;
        private readonly INotificationService _notifications;

        public ContractsController(CrmDbContext db, IWebHostEnvironment env, INotificationService notifications)
        {
            _db = db;
            _env = env;
            _notifications = notifications;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contract>>> GetAll()
        {
            var contracts = await _db.Contracts
                .Include(ct => ct.Company)
                .ToListAsync();
            return Ok(contracts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contract>> GetById(int id)
        {
            var contract = await _db.Contracts
                .Include(ct => ct.Company)
                .FirstOrDefaultAsync(ct => ct.Id == id);

            if (contract == null)
                return NotFound();

            return Ok(contract);
        }

        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<ActionResult<Contract>> Create([FromForm] ContractUploadDto dto)
        {
            var contract = new Contract
            {
                CompanyId = dto.CompanyId,
                ProjectId = dto.ProjectId,
                Reference = dto.Reference,
                Version = dto.Version,
                DateStart = dto.DateStart,
                DateEnd = dto.DateEnd,
                Amount = dto.Amount,
                Status = dto.Status ?? "draft",
                Notes = dto.Notes,
                UploadDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
            };

            // Handle PDF file upload
            if (dto.File != null && dto.File.Length > 0)
            {
                var allowedExtensions = new[] { ".pdf", ".docx", ".doc" };
                var extension = Path.GetExtension(dto.File.FileName).ToLowerInvariant();
                if (!allowedExtensions.Contains(extension))
                    return BadRequest(new { message = "Type de fichier non autorisé. Formats acceptés: PDF, DOCX, DOC." });

                if (dto.File.Length > 20 * 1024 * 1024)
                    return BadRequest(new { message = "Le fichier dépasse la taille maximale de 20 Mo." });

                var uploadsDir = Path.Combine(_env.ContentRootPath, "Uploads", "Contracts");
                Directory.CreateDirectory(uploadsDir);

                var fileName = $"{contract.Reference}-V{contract.Version}_{DateTime.UtcNow:yyyyMMddHHmmss}{Path.GetExtension(dto.File.FileName)}";
                var filePath = Path.Combine(uploadsDir, fileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await dto.File.CopyToAsync(stream);
                }

                contract.FilePath = $"/uploads/contracts/{fileName}";
            }

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();

            var title = contract.Version > 1
                ? $"Contrat V{contract.Version} téléversé"
                : "Nouveau contrat téléversé";
            var link = contract.ProjectId.HasValue
                ? $"/crm/pipeline?opp={contract.ProjectId}"
                : "/crm/contracts";
            await _notifications.NotifyAllAsync(
                "contract_uploaded",
                title,
                $"Référence {contract.Reference} — version V{contract.Version}.",
                link);

            return CreatedAtAction(nameof(GetById), new { id = contract.Id }, contract);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Contract>> Update(int id, Contract updatedContract)
        {
            var existing = await _db.Contracts.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Reference = updatedContract.Reference;
            existing.Version = updatedContract.Version;
            existing.DateStart = updatedContract.DateStart;
            existing.DateEnd = updatedContract.DateEnd;
            existing.Amount = updatedContract.Amount;
            existing.Status = updatedContract.Status;
            existing.UploadedById = updatedContract.UploadedById;
            existing.UploadDate = updatedContract.UploadDate;
            existing.Notes = updatedContract.Notes;
            existing.CompanyId = updatedContract.CompanyId;
            existing.ProjectId = updatedContract.ProjectId;
            existing.FilePath = updatedContract.FilePath;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Contracts.FindAsync(id);
            if (existing == null)
                return NotFound();

            // Delete associated file
            if (!string.IsNullOrEmpty(existing.FilePath))
            {
                var fullPath = Path.Combine(_env.ContentRootPath, "Uploads", "Contracts",
                    Path.GetFileName(existing.FilePath));
                if (System.IO.File.Exists(fullPath))
                    System.IO.File.Delete(fullPath);
            }

            _db.Contracts.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<Contract>>> GetByCompany(int companyId)
        {
            var contracts = await _db.Contracts
                .Where(ct => ct.CompanyId == companyId)
                .ToListAsync();
            return Ok(contracts);
        }

        [HttpGet("ByProject/{projectId}")]
        public async Task<ActionResult<IEnumerable<Contract>>> GetByProject(int projectId)
        {
            var contracts = await _db.Contracts
                .Where(ct => ct.ProjectId == projectId)
                .Include(ct => ct.Company)
                .ToListAsync();
            return Ok(contracts);
        }

        [HttpGet("download/{id}")]
        public async Task<ActionResult> Download(int id)
        {
            var contract = await _db.Contracts.FindAsync(id);
            if (contract == null || string.IsNullOrEmpty(contract.FilePath))
                return NotFound();

            var fullPath = Path.Combine(_env.ContentRootPath, "Uploads", "Contracts",
                Path.GetFileName(contract.FilePath));

            if (!System.IO.File.Exists(fullPath))
                return NotFound("Fichier introuvable");

            var bytes = await System.IO.File.ReadAllBytesAsync(fullPath);
            return File(bytes, "application/pdf", Path.GetFileName(contract.FilePath));
        }
    }

    public class ContractUploadDto
    {
        public int CompanyId { get; set; }
        public int? ProjectId { get; set; }
        public string Reference { get; set; } = string.Empty;
        public int Version { get; set; } = 1;
        public DateTime? DateStart { get; set; }
        public DateTime? DateEnd { get; set; }
        public decimal Amount { get; set; }
        public string? Status { get; set; }
        public string? Notes { get; set; }
        public IFormFile? File { get; set; }
    }
}
