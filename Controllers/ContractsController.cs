using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContractsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public ContractsController(CrmDbContext db)
        {
            _db = db;
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
        public async Task<ActionResult<Contract>> Create(Contract contract)
        {
            contract.CreatedAt = DateTime.UtcNow;
            contract.UpdatedAt = DateTime.UtcNow;

            _db.Contracts.Add(contract);
            await _db.SaveChangesAsync();

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
    }
}
