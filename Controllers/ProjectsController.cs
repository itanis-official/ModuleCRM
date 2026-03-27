using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProjectsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public ProjectsController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Project>>> GetAll()
        {
            var projects = await _db.Projects
                .Include(p => p.Company)
                .ToListAsync();
            return Ok(projects);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Project>> GetById(int id)
        {
            var project = await _db.Projects
                .Include(p => p.Company)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (project == null)
                return NotFound();

            return Ok(project);
        }

        [HttpPost]
        public async Task<ActionResult<Project>> Create(Project project)
        {
            project.CreatedAt = DateTime.UtcNow;
            project.UpdatedAt = DateTime.UtcNow;

            _db.Projects.Add(project);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<Project>> Update(int id, Project updatedProject)
        {
            var existing = await _db.Projects.FindAsync(id);
            if (existing == null)
                return NotFound();

            existing.Name = updatedProject.Name;
            existing.Reference = updatedProject.Reference;
            existing.Description = updatedProject.Description;
            existing.Status = updatedProject.Status;
            existing.StartDate = updatedProject.StartDate;
            existing.EndDate = updatedProject.EndDate;
            existing.CompanyId = updatedProject.CompanyId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            return Ok(existing);
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> Delete(int id)
        {
            var existing = await _db.Projects.FindAsync(id);
            if (existing == null)
                return NotFound();

            _db.Projects.Remove(existing);
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("ByCompany/{companyId}")]
        public async Task<ActionResult<IEnumerable<Project>>> GetByCompany(int companyId)
        {
            var projects = await _db.Projects
                .Where(p => p.CompanyId == companyId)
                .ToListAsync();

            return Ok(projects);
        }
    }
}
