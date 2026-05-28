using System.Security.Claims;

namespace ModuleCRM.Services
{
    /// <summary>
    /// Extensions pour lire le role ITANIS depuis le token JWT.
    /// Lit en priorite les groupes Authentik, fallback sur l'ancien claim role.
    /// </summary>
    public static class ClaimsExtensions
    {
        // Mapping groupe Authentik -> role interne ITANIS.
        // Fige par DevOps (voir doc integration-authentik-RH.pdf).
        private static readonly Dictionary<string, string> GroupToRole = new(StringComparer.OrdinalIgnoreCase)
        {
            ["ceo"]              = "super_admin",
            ["commercial"]       = "agent_commercial",
            ["resp-projet"]      = "chef_projet",
            ["rh-manager"]       = "rh",
            ["support-helpdesk"] = "agent",
            ["timesheet"]        = "agent",
            ["analyste-bi"]      = "agent",
            ["client-crm"]       = "contact",
        };

        // Ordre de priorite si l'utilisateur a plusieurs groupes (le plus eleve gagne)
        private static readonly string[] RolePriority =
        {
            "super_admin", "rh", "chef_projet", "agent_commercial", "contact", "agent"
        };

        /// <summary>
        /// Resout le role ITANIS depuis les claims du token.
        /// Priorite : claim Authentik "groups" -> ancien claim "role" -> "agent" par defaut.
        /// </summary>
        public static string GetItanisRole(this ClaimsPrincipal user)
        {
            // 1) Token Authentik : claim "groups" (array de strings)
            var groups = user.FindAll("groups").Select(c => c.Value).ToList();
            if (groups.Count > 0)
            {
                var mappedRoles = groups
                    .Where(g => GroupToRole.ContainsKey(g))
                    .Select(g => GroupToRole[g])
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                foreach (var r in RolePriority)
                    if (mappedRoles.Contains(r)) return r;
            }

            // 2) Fallback ancien systeme JWT local
            var legacy = user.FindFirstValue(ClaimTypes.Role) ?? user.FindFirstValue("role");
            return NormalizeLegacyRole(legacy);
        }

        private static string NormalizeLegacyRole(string? rawRole)
        {
            if (string.IsNullOrWhiteSpace(rawRole)) return "agent";
            var token = rawRole.Normalize(System.Text.NormalizationForm.FormD).ToLowerInvariant();
            token = System.Text.RegularExpressions.Regex.Replace(token, "[^a-z0-9]", "");

            if (token.Contains("superadmin") || token == "admin" || token.Contains("administrateur")) return "super_admin";
            if (token.Contains("rh") || token.Contains("ressourceshumaines")) return "rh";
            if (token.Contains("chefprojet") || token.Contains("manager") || token.Contains("lead")) return "chef_projet";
            if (token.Contains("commercial")) return "agent_commercial";
            if (token.Contains("contact") || token.Contains("client")) return "contact";
            return "agent";
        }
    }
}
