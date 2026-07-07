using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using ModuleCRM.Data;
using ModuleCRM.Models;

namespace ModuleCRM.Services
{
    // Stocke un fichier uploade en base (table DocumentsCrm) et renvoie l'URL relative
    // de l'endpoint authentifie qui le sert (consommee telle quelle par crmApi cote front).
    public static class DocumentStorage
    {
        public static async Task<string> SaveAsync(CrmDbContext db, IFormFile file)
        {
            using var ms = new MemoryStream();
            await file.CopyToAsync(ms);

            var doc = new DocumentCrm
            {
                NomFichier = Path.GetFileName(file.FileName),
                TypeContenu = string.IsNullOrWhiteSpace(file.ContentType) ? "application/octet-stream" : file.ContentType,
                Donnees = ms.ToArray(),
                DateUpload = DateTime.UtcNow,
            };
            db.DocumentsCrm.Add(doc);
            await db.SaveChangesAsync();

            // Relatif a la base crmApi (/api/crm) → /api/crm/documents/{id}
            return $"/documents/{doc.Id}";
        }
    }
}
