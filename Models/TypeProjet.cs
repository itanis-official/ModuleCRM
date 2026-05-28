using System.ComponentModel.DataAnnotations;

namespace ModuleCRM.Models
{
    public class TypeProjet
    {
        public int Id { get; set; }

        public Guid TypeProjetGuid { get; set; } = Guid.NewGuid();

        [Required]
        [StringLength(50)]
        public string Value { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Label { get; set; } = string.Empty;

        public bool IsActive { get; set; } = true;
        public int Ordre { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
