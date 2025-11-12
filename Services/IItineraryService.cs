using travel_planner.DTOs;

namespace travel_planner.Services
{
    public interface IItineraryService
    {
        Task<List<ItineraryDto>> GetTripItinerariesAsync(int tripId, int userId);
        Task<ItineraryDto?> GetItineraryByIdAsync(int itineraryId, int userId);
        Task<ItineraryDto?> CreateItineraryAsync(int tripId, CreateItineraryDto dto, int userId);
        Task<ItineraryDto?> UpdateItineraryAsync(int itineraryId, UpdateItineraryDto dto, int userId);
        Task<bool> DeleteItineraryAsync(int itineraryId, int userId);
    }
}