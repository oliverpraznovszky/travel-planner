using System.ComponentModel.DataAnnotations;
using travel_planner.Models;

namespace travel_planner.DTOs
{
    public class UpdateLocationDto
    {
        [Required]
        [StringLength(200, MinimumLength = 2)]
        public string Name { get; set; } = string.Empty;

        [StringLength(500)]
        public string? Address { get; set; }

        [Range(-90, 90)]
        public double? Latitude { get; set; }

        [Range(-180, 180)]
        public double? Longitude { get; set; }

        public LocationType Type { get; set; }
    }
}