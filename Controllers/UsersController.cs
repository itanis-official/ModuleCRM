using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public UsersController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetAll()
        {
            var users = await _db.Users.ToListAsync();
            return Ok(users);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetById(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
                return NotFound();
            return Ok(user);
        }

        [HttpPost]
        public async Task<ActionResult<User>> Create(User user)
        {
            user.CreatedAt = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = user.Id }, user);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<User>> Update(int id, User updatedUser)
        {
            var existing = await _db.Users.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Nom = updatedUser.Nom;
            existing.Prenom = updatedUser.Prenom;
            existing.Email = updatedUser.Email;
            existing.Telephone = updatedUser.Telephone;
            existing.Login = updatedUser.Login;
            existing.PasswordHash = updatedUser.PasswordHash;
            existing.Avatar = updatedUser.Avatar;
            existing.Role = updatedUser.Role;
            existing.IsActive = updatedUser.IsActive;
            existing.LastLogin = updatedUser.LastLogin;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Users.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Users.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }
}
