using System.Net.Http.Json;
using ModuleCRM.DTOs;

namespace ModuleCRM.Services
{
    public class AgentApiService
    {
        private readonly HttpClient _httpClient;

        public AgentApiService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<AgentDto>> GetAllAsync()
        {
            var agents = await _httpClient.GetFromJsonAsync<List<AgentDto>>("api/agents");
            return agents ?? new List<AgentDto>();
        }

        public async Task<AgentDto?> GetByIdAsync(int id)
        {
            try
            {
                return await _httpClient.GetFromJsonAsync<AgentDto>($"api/agents/{id}");
            }
            catch (HttpRequestException)
            {
                return null;
            }
        }

        public async Task<List<AgentDto>> GetByRoleAsync(string role)
        {
            var agents = await _httpClient.GetFromJsonAsync<List<AgentDto>>($"api/agents/by-role/{role}");
            return agents ?? new List<AgentDto>();
        }

        public async Task<List<AgentDto>> GetByDepartementAsync(string departement)
        {
            var agents = await _httpClient.GetFromJsonAsync<List<AgentDto>>($"api/agents/by-departement/{departement}");
            return agents ?? new List<AgentDto>();
        }
    }
}
