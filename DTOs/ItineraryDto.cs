namespace travel_planner.DTOs
{
    public class ItineraryDto
    {
        public int Id { get; set; }
        public int TripId { get; set; }
        public int DayNumber { get; set; }
        public DateTime Date { get; set; }
        public string? Notes { get; set; }
        public List<ActivityDto> Activities { get; set; } = new();
    }
}