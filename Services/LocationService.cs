using Microsoft.EntityFrameworkCore;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class LocationService : ILocationService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripService _tripService;

        public LocationService(ApplicationDbContext context, ITripService tripService)
        {
            _context = context;
            _tripService = tripService;
        }

        public async Task<List<LocationDto>> GetTripLocationsAsync(int tripId, int userId)
        {
            if (!await _tripService.UserHasAccessToTripAsync(tripId, userId))
            {
                return new List<LocationDto>();
            }

            var locations = await _context.Locations
                .Where(l => l.TripId == tripId)
                .OrderBy(l => l.CreatedAt)
                .Select(l => new LocationDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Address = l.Address,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Type = l.Type,
                    CreatedAt = l.CreatedAt
                })
                .ToListAsync();

            return locations;
        }

        public async Task<LocationDto?> GetLocationByIdAsync(int locationId, int userId)
        {
            var location = await _context.Locations
                .Include(l => l.Trip)
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null || !await _tripService.UserHasAccessToTripAsync(location.TripId, userId))
            {
                return null;
            }

            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Type = location.Type,
                CreatedAt = location.CreatedAt
            };
        }

        public async Task<LocationDto?> CreateLocationAsync(int tripId, CreateLocationDto dto, int userId)
        {
            if (!await _tripService.UserCanEditTripAsync(tripId, userId))
            {
                return null;
            }

            var location = new Location
            {
                Name = dto.Name,
                Address = dto.Address,
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                Type = dto.Type,
                TripId = tripId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Locations.Add(location);
            await _context.SaveChangesAsync();

            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Type = location.Type,
                CreatedAt = location.CreatedAt
            };
        }

        public async Task<LocationDto?> UpdateLocationAsync(int locationId, UpdateLocationDto dto, int userId)
        {
            var location = await _context.Locations
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null || !await _tripService.UserCanEditTripAsync(location.TripId, userId))
            {
                return null;
            }

            location.Name = dto.Name;
            location.Address = dto.Address;
            location.Latitude = dto.Latitude;
            location.Longitude = dto.Longitude;
            location.Type = dto.Type;

            await _context.SaveChangesAsync();

            return new LocationDto
            {
                Id = location.Id,
                Name = location.Name,
                Address = location.Address,
                Latitude = location.Latitude,
                Longitude = location.Longitude,
                Type = location.Type,
                CreatedAt = location.CreatedAt
            };
        }

        public async Task<bool> DeleteLocationAsync(int locationId, int userId)
        {
            var location = await _context.Locations
                .Include(l => l.Activities)
                .FirstOrDefaultAsync(l => l.Id == locationId);

            if (location == null || !await _tripService.UserCanEditTripAsync(location.TripId, userId))
            {
                return false;
            }

            // Ha van Activity ami használja ezt a Location-t, null-ra állítjuk
            foreach (var activity in location.Activities)
            {
                activity.LocationId = null;
            }

            _context.Locations.Remove(location);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}