using System.ComponentModel.DataAnnotations;
using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class AddParticipantDto
    {
        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public ParticipantRole Role { get; set; } = ParticipantRole.Viewer;
    }
}