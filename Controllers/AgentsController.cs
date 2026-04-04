using Microsoft.AspNetCore.Mvc;
using ModuleCRM.DTOs;
using ModuleCRM.Services;

namespace ModuleCRM.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly AgentApiService _agentService;

        public AgentsController(AgentApiService agentService)
        {
            _agentService = agentService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetAll()
        {
            var agents = await _agentService.GetAllAsync();
            return Ok(agents);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<AgentDto>> GetById(int id)
        {
            var agent = await _agentService.GetByIdAsync(id);
            if (agent == null)
                return NotFound();
            return Ok(agent);
        }

        [HttpGet("by-role/{role}")]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetByRole(string role)
        {
            var agents = await _agentService.GetByRoleAsync(role);
            return Ok(agents);
        }

        [HttpGet("by-departement/{departement}")]
        public async Task<ActionResult<IEnumerable<AgentDto>>> GetByDepartement(string departement)
        {
            var agents = await _agentService.GetByDepartementAsync(departement);
            return Ok(agents);
        }
    }
}
