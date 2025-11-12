using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class ActivityDto
    {
        public int Id { get; set; }
        public int ItineraryId { get; set; }
        public int? LocationId { get; set; }
        public string? LocationName { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public TimeSpan? StartTime { get; set; }
        public TimeSpan? EndTime { get; set; }
        public decimal? EstimatedCost { get; set; }
        public ActivityPriority Priority { get; set; }
        public int OrderIndex { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}