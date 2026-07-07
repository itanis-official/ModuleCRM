using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleCRM.Data;

namespace ModuleCRM.Controllers
{
    // Sert les documents CRM stockes en base. Authentifie : le front appelle avec le token
    // (crmApi.get(..., { responseType: 'blob' })) puis ouvre le blob → marche en prod, sans /uploads.
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentsController : ControllerBase
    {
        private readonly CrmDbContext _db;

        public DocumentsController(CrmDbContext db)
        {
            _db = db;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var doc = await _db.DocumentsCrm.FindAsync(id);
            if (doc == null) return NotFound();

            Response.Headers["Content-Disposition"] = $"inline; filename=\"{doc.NomFichier}\"";
            return File(doc.Donnees, doc.TypeContenu);
        }
    }
}
