using System;
using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    // Document CRM (CDC, contrat, proposition, doc de phase) stocke en base (varbinary)
    // au lieu du disque ephemere du conteneur. Servi via l'endpoint authentifie
    // /api/crm/documents/{id} : survit aux redeploiements, plus de dependance /uploads.
    public class DocumentCrm
    {
        public int Id { get; set; }

        [MaxLength(260)]
        public string NomFichier { get; set; } = string.Empty;

        [MaxLength(150)]
        public string TypeContenu { get; set; } = "application/octet-stream";

        public byte[] Donnees { get; set; } = Array.Empty<byte>();

        public DateTime DateUpload { get; set; } = DateTime.UtcNow;
    }
}
