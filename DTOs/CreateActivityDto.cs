using System.ComponentModel.DataAnnotations;
using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class CreateActivityDto
    {
        public int? LocationId { get; set; }

        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Title { get; set; } = string.Empty;

        [StringLength(1000)]
        public string? Description { get; set; }

        public TimeSpan? StartTime { get; set; }

        public TimeSpan? EndTime { get; set; }

        [Range(0, double.MaxValue)]
        public decimal? EstimatedCost { get; set; }

        public ActivityPriority Priority { get; set; } = ActivityPriority.Medium;

        public int OrderIndex { get; set; } = 0;
    }
}