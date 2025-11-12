using System.ComponentModel.DataAnnotations;

namespace travel_planner.DTOs
{
    public class UpdateItineraryDto
    {
        [Required]
        public DateTime Date { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}