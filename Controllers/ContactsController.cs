using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContactsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public ContactsController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Contact>>> GetAll()
        {
            var contacts = await _db.Contacts
                .Include(c => c.Company)
                .ToListAsync();

            return Ok(contacts);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Contact>> GetById(int id)
        {
            var contact = await _db.Contacts
                .Include(c => c.Company)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (contact == null)
                return NotFound();

            return Ok(contact);
        }

        [HttpPost]
        public async Task<ActionResult<Contact>> Create(Contact contact)
        {
            contact.CreatedAt = DateTime.UtcNow;
            contact.UpdatedAt = DateTime.UtcNow;

            _db.Contacts.Add(contact);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Contact>> Update(int id, Contact updatedContact)
        {
            var existing = await _db.Contacts.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Nom = updatedContact.Nom;
            existing.Prenom = updatedContact.Prenom;
            existing.Poste = updatedContact.Poste;
            existing.Email = updatedContact.Email;
            existing.Telephone = updatedContact.Telephone;
            existing.TelephoneCountry = updatedContact.TelephoneCountry;
            existing.Login = updatedContact.Login;
            existing.PasswordHash = updatedContact.PasswordHash;
            existing.SendEmail = updatedContact.SendEmail;
            existing.ForcePasswordChange = updatedContact.ForcePasswordChange;
            existing.IsActive = updatedContact.IsActive;
            existing.LastLogin = updatedContact.LastLogin;
            existing.CompanyId = updatedContact.CompanyId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Contacts.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Contacts.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<Contact>>> GetByCompany(int companyId)
        {
            var contacts = await _db.Contacts
                .Where(c => c.CompanyId == companyId)
                .ToListAsync();

            return Ok(contacts);
        }
    }
}
