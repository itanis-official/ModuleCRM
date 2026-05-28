using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ModuleCRM.Models
{
    /// <summary>
    /// Réunion client liée à une phase "meeting" d'opportunité.
    /// Une phase meeting peut contenir plusieurs réunions (kick-off, intermédiaire, etc.).
    /// </summary>
    public class Meeting
    {
        public int Id { get; set; }

        public int PhaseId { get; set; }
        [JsonIgnore]
        public Phase? Phase { get; set; }

        [Required]
        public DateTime Date { get; set; }

        /// <summary>Heure au format "HH:mm" (string pour souplesse côté UI).</summary>
        public string? Time { get; set; }

        [StringLength(200)]
        public string? Title { get; set; }

        [StringLength(500)]
        public string? Lieu { get; set; }

        public string? Notes { get; set; }

        /// <summary>Liste libre des participants (noms séparés par virgule, ou JSON si besoin).</summary>
        public string? Participants { get; set; }

        public bool Done { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
