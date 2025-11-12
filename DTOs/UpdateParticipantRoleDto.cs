using System.ComponentModel.DataAnnotations;
using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class UpdateParticipantRoleDto
    {
        [Required]
        public ParticipantRole Role { get; set; }
    }
}