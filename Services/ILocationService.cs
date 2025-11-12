using travel_planner.DTOs;

namespace travel_planner.Services
{
    public interface ILocationService
    {
        Task<List<LocationDto>> GetTripLocationsAsync(int tripId, int userId);
        Task<LocationDto?> GetLocationByIdAsync(int locationId, int userId);
        Task<LocationDto?> CreateLocationAsync(int tripId, CreateLocationDto dto, int userId);
        Task<LocationDto?> UpdateLocationAsync(int locationId, UpdateLocationDto dto, int userId);
        Task<bool> DeleteLocationAsync(int locationId, int userId);
    }
}