using System.ComponentModel.DataAnnotations;

namespace travel_planner.DTOs
{
    public class CreateItineraryDto
    {
        [Required]
        [Range(1, int.MaxValue)]
        public int DayNumber { get; set; }

        [Required]
        public DateTime Date { get; set; }

        [StringLength(1000)]
        public string? Notes { get; set; }
    }
}