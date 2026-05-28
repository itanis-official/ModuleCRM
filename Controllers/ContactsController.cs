using System.Security.Claims;
using ITANIS.SharedEvents;
using MassTransit;
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
    public class ContactsController : ControllerBase
    {
        private readonly CrmDbContext _db;
        private readonly ILogger<ContactsController> _logger;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly AuthentikService _authentikService;

        public ContactsController(
            CrmDbContext db,
            ILogger<ContactsController> logger,
            IPublishEndpoint publishEndpoint,
            AuthentikService authentikService)
        {
            _db = db;
            _logger = logger;
            _publishEndpoint = publishEndpoint;
            _authentikService = authentikService;
        }

        private string NormalizeRole(string? rawRole)
        {
            if (string.IsNullOrWhiteSpace(rawRole)) return "agent";
            var token = rawRole.Normalize(System.Text.NormalizationForm.FormD).ToLowerInvariant();
            token = System.Text.RegularExpressions.Regex.Replace(token, "[^a-z0-9]", "");

            if (token.Contains("superadmin") || token == "admin" || token.Contains("administrateur")) return "super_admin";
            if (token.Contains("rh") || token.Contains("ressourceshumaines")) return "rh";
            if (token.Contains("chefprojet") || token.Contains("manager") || token.Contains("lead") || (token.Contains("chef") && token.Contains("projet"))) return "chef_projet";
            if (token.Contains("commercial")) return "agent_commercial";
            if (token.Contains("contact") || token.Contains("client")) return "contact";
            
            return "agent";
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
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent créer des contacts.");
            }
            try
            {
                // Authentik gere le password (plus de hash local).
                contact.PasswordHash = string.Empty;
                contact.CreatedAt = DateTime.UtcNow;
                contact.UpdatedAt = DateTime.UtcNow;

                _db.Contacts.Add(contact);
                await _db.SaveChangesAsync();

                await PublishSync(contact, SyncAction.Created);

                // Authentik : creer le compte de connexion (groupe client-crm).
                // Ordre db_crm -> RabbitMQ -> Authentik. Si Authentik echoue, on log mais
                // on ne rollback pas la BD (le contact existe, on retry plus tard).
                try
                {
                    var displayName = $"{contact.Prenom} {contact.Nom}".Trim();
                    var username = contact.Login ?? contact.Email;
                    await _authentikService.CreateClientUserAsync(username, displayName, contact.Email);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "Echec creation Authentik pour contact {ContactId} ({Email}). " +
                        "Contact cree en BD mais ne pourra pas se connecter. A retry.",
                        contact.Id, contact.Email);
                }

                return CreatedAtAction(nameof(GetById), new { id = contact.Id }, contact);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la creation du contact");
                // Return details in development to help debugging (can be tightened for production)
                return StatusCode(500, new { message = "Erreur serveur lors de la création du contact.", detail = ex.Message });
            }
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Contact>> Update(int id, Contact updatedContact)
        {
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent modifier des contacts.");
            }

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

            await PublishSync(existing, SyncAction.Updated);

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var userRole = User.GetItanisRole();

            if (userRole != "super_admin" && userRole != "agent_commercial")
            {
                return Forbid("Seuls les administrateurs et agents commerciaux peuvent supprimer des contacts.");
            }

            var existing = await _db.Contacts.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Contacts.Remove(existing);
            await _db.SaveChangesAsync();

            await PublishSync(existing, SyncAction.Deleted);

            return NoContent();
        }

        private Task PublishSync(Contact c, SyncAction action)
        {
            return _publishEndpoint.Publish(new ContactSyncEvent
            {
                Action = action,
                Id = c.Id,
                CompanyId = c.CompanyId,
                Nom = c.Nom,
                Prenom = c.Prenom,
                Poste = c.Poste,
                Email = c.Email,
                Telephone = c.Telephone,
                TelephoneCountry = c.TelephoneCountry,
                IsActive = c.IsActive,
                ChangedAt = DateTime.UtcNow,
            });
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
