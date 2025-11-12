using travel_planner.DTOs;

namespace travel_planner.Services
{
    public interface ITripService
    {
        Task<List<TripDto>> GetUserTripsAsync(int userId);
        Task<TripDetailDto?> GetTripByIdAsync(int tripId, int userId);
        Task<TripDto?> CreateTripAsync(CreateTripDto createTripDto, int userId);
        Task<TripDto?> UpdateTripAsync(int tripId, UpdateTripDto updateTripDto, int userId);
        Task<bool> DeleteTripAsync(int tripId, int userId);
        Task<bool> UserHasAccessToTripAsync(int tripId, int userId);
        Task<bool> UserCanEditTripAsync(int tripId, int userId);
    }
}