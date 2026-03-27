using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompaniesController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public CompaniesController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Company>>> GetAll()
        {
            var companies = await _db.Companies
                .Include(c => c.AgentResponsable)
                .Include(c => c.Contacts)
                .Include(c => c.Projects)
                .ToListAsync();
            return Ok(companies);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Company>> GetById(int id)
        {
            var company = await _db.Companies
                .Include(c => c.AgentResponsable)
                .Include(c => c.Contacts)
                .Include(c => c.Projects)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (company == null)
                return NotFound();

            return Ok(company);
        }

        [HttpPost]
        public async Task<ActionResult<Company>> Create(Company company)
        {
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
    }
}
