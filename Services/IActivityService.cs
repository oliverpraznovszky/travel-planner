using travel_planner.DTOs;

namespace travel_planner.Services
{
    public interface IActivityService
    {
        Task<ActivityDto?> GetActivityByIdAsync(int activityId, int userId);
        Task<ActivityDto?> CreateActivityAsync(int itineraryId, CreateActivityDto dto, int userId);
        Task<ActivityDto?> UpdateActivityAsync(int activityId, UpdateActivityDto dto, int userId);
        Task<bool> DeleteActivityAsync(int activityId, int userId);
    }
}