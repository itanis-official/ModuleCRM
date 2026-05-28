using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;

namespace ModuleCRM.Services
{
    /// <summary>
    /// Crée les utilisateurs CRM (contacts clients) dans Authentik via son API REST.
    /// Tous les contacts CRM sont placés dans un seul groupe (client-crm).
    /// </summary>
    public class AuthentikService
    {
        private readonly HttpClient _http;
        private readonly ILogger<AuthentikService> _logger;
        private readonly IConfiguration _config;
        private readonly string _clientGroupUuid;

        public AuthentikService(HttpClient http, IConfiguration config, ILogger<AuthentikService> logger)
        {
            _http = http;
            _logger = logger;
            _config = config;

            // UUID du groupe Authentik "client-crm" (fixe pour le CRM)
            _clientGroupUuid = config["Authentik:ClientGroupUuid"]
                ?? "51286497-6666-4b8a-84f9-d075d5b38732";

            // BaseURL configurée via appsettings (ou défaut interne au cluster K8s)
            var baseUrl = config["Authentik:BaseUrl"]
                ?? "http://authentik-server.authentik.svc.cluster.local";
            _http.BaseAddress = new Uri(baseUrl);
        }

        // Lit le token Authentik depuis env var (prod K8s) OU IConfiguration (dev local user-secrets)
        private string? ReadToken()
            => Environment.GetEnvironmentVariable("AUTHENTIK_API_TOKEN")
            ?? _config["AUTHENTIK_API_TOKEN"]
            ?? _config["Authentik:ApiToken"];

        /// <summary>
        /// Crée un user dans Authentik et retourne son pk (id Authentik).
        /// </summary>
        public async Task<string> CreateClientUserAsync(string username, string name, string email)
        {
            var token = ReadToken();
            if (string.IsNullOrWhiteSpace(token))
                throw new InvalidOperationException(
                    "AUTHENTIK_API_TOKEN n'est pas défini. " +
                    "En dev local : dotnet user-secrets set \"AUTHENTIK_API_TOKEN\" \"<token>\"");

            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            var payload = new
            {
                username,
                name,
                email,
                is_active = true,
                type = "external",
                groups = new[] { _clientGroupUuid }
            };

            var response = await _http.PostAsJsonAsync("/api/v3/core/users/", payload);

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError(
                    "Authentik CreateClientUserAsync échoué : status={Status} body={Body}",
                    (int)response.StatusCode, body);
                throw new HttpRequestException(
                    $"Authentik returned {(int)response.StatusCode}: {body}");
            }

            var created = await response.Content.ReadFromJsonAsync<JsonElement>();
            var pk = created.GetProperty("pk").ToString();

            _logger.LogInformation(
                "Utilisateur Authentik créé : username={Username} pk={Pk} groupe={Group}",
                username, pk, _clientGroupUuid);

            return pk;
        }
    }
}
