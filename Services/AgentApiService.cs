using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;

namespace ModuleCRM.Services
{
    /// <summary>
    /// Source de verite : table locale AgentsLocal (read-replica RH via RabbitMQ).
    /// Auth migree vers Authentik : plus de Request/Reply MassTransit auth/profile/changePassword.
    /// </summary>
    public class AgentApiService
    {
        private readonly CrmDbContext _db;

        public AgentApiService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<List<AgentDto>> GetAllAsync()
        {
            return await _db.AgentsLocal
                .AsNoTracking()
                .OrderBy(a => a.Nom).ThenBy(a => a.Prenom)
                .Select(a => Map(a))
                .ToListAsync();
        }

        public async Task<AgentDto?> GetByIdAsync(int id)
        {
            var a = await _db.AgentsLocal.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            return a == null ? null : Map(a);
        }

        public async Task<List<AgentDto>> GetByRoleAsync(string role)
        {
            return await _db.AgentsLocal
                .AsNoTracking()
                .Where(a => a.Role == role)
                .Select(a => Map(a))
                .ToListAsync();
        }

        public async Task<List<AgentDto>> GetByDepartementAsync(string departement)
        {
            return await _db.AgentsLocal
                .AsNoTracking()
                .Where(a => a.Departement == departement)
                .Select(a => Map(a))
                .ToListAsync();
        }

        private static AgentDto Map(Models.AgentLocal a) => new AgentDto
        {
            Id = a.Id,
            Nom = a.Nom,
            Prenom = a.Prenom,
            Email = a.Email,
            Telephone = a.Telephone,
            Role = a.Role,
            IsActive = a.IsActive,
            Poste = a.Poste,
            Departement = a.Departement,
            Statut = a.IsActive ? "actif" : "inactif",
            Type = a.AgentType,
        };
    }
}
