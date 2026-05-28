using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ModuleCRM.DTOs;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipesController : ControllerBase
    {
        private readonly EquipeApiService _equipeService;

        public EquipesController(EquipeApiService equipeService)
        {
            _equipeService = equipeService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EquipeDto>>> GetAll()
        {
            var equipes = await _equipeService.GetAllAsync();
            return Ok(equipes.Where(e => e.IsActive));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EquipeDto>> GetById(int id)
        {
            var equipe = await _equipeService.GetByIdAsync(id);
            if (equipe == null)
                return NotFound();
            return Ok(equipe);
        }
    }
}
