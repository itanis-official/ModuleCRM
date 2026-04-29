using System.Net.Http.Json;
using ModuleCRM.DTOs;

namespace ModuleCRM.Services
{
    public class EquipeApiService
    {
        private readonly HttpClient _httpClient;

        public EquipeApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<EquipeDto>> GetAllAsync()
        {
            var equipes = await _httpClient.GetFromJsonAsync<List<EquipeDto>>("api/equipes");
            return equipes ?? new List<EquipeDto>();
        }

        public async Task<EquipeDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<EquipeDto>($"api/equipes/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }
    }
}
