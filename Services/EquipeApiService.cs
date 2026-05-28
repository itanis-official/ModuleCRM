using Microsoft.EntityFrameworkCore;
using ModuleCRM.Data;
using ModuleCRM.DTOs;
using ModuleCRM.Models;

namespace ModuleCRM.Services
{
    /// <summary>
    /// Lecture des équipes depuis la table locale EquipesLocal (read-replica
    /// alimentée par EquipeSyncEvent via RabbitMQ).
    /// </summary>
    public class EquipeApiService
    {
        private readonly CrmDbContext _db;

        public EquipeApiService(CrmDbContext db)
        {
            _db = db;
        }

        public async Task<List<EquipeDto>> GetAllAsync()
        {
            var equipes = await _db.EquipesLocal
                .AsNoTracking()
                .OrderBy(e => e.Nom)
                .ToListAsync();

            return await BuildDtosAsync(equipes);
        }

        public async Task<EquipeDto?> GetByIdAsync(int id)
        {
            // L'ID exposé côté CRM correspond à IdOrigine (l'ID source RH/GestionProjet)
            var equipe = await _db.EquipesLocal
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.IdOrigine == id);

            if (equipe == null) return null;
            var dtos = await BuildDtosAsync(new List<EquipeLocal> { equipe });
            return dtos.FirstOrDefault();
        }

        private async Task<List<EquipeDto>> BuildDtosAsync(List<EquipeLocal> equipes)
        {
            var chefIds = equipes
                .Where(e => e.ChefProjetIdOrigine.HasValue)
                .Select(e => e.ChefProjetIdOrigine!.Value)
                .Distinct()
                .ToList();

            var chefs = await _db.AgentsLocal
                .AsNoTracking()
                .Where(a => chefIds.Contains(a.Id))
                .ToDictionaryAsync(a => a.Id, a => a);

            return equipes.Select(e =>
            {
                ChefProjetDto? chefDto = null;
                if (e.ChefProjetIdOrigine.HasValue && chefs.TryGetValue(e.ChefProjetIdOrigine.Value, out var c))
                {
                    chefDto = new ChefProjetDto
                    {
                        Id = c.Id,
                        Nom = c.Nom,
                        Prenom = c.Prenom,
                        Email = c.Email,
                    };
                }

                return new EquipeDto
                {
                    Id = e.IdOrigine,
                    Nom = e.Nom,
                    Domaine = e.Domaine,
                    Description = e.Description,
                    IsActive = e.IsActive,
                    ChefProjet = chefDto,
                };
            }).ToList();
        }
    }
}
