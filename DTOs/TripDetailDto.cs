using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class TripDetailDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public string? Description { get; set; }
        public decimal Budget { get; set; }
        public int CreatedById { get; set; }
        public string CreatedByUsername { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }

        public List<ParticipantDto> Participants { get; set; } = new();
        public List<LocationDto> Locations { get; set; } = new();
        public int ItineraryCount { get; set; }
    }

    public class ParticipantDto
    {
        public int UserId { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public ParticipantRole Role { get; set; }
        public DateTime JoinedAt { get; set; }
    }

    public class LocationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Address { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public LocationType Type { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}