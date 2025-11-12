using travel_planner.DTOs;

namespace travel_planner.Services
{
    public interface IParticipantService
    {
        Task<ParticipantDto?> AddParticipantAsync(int tripId, AddParticipantDto dto, int currentUserId);
        Task<bool> RemoveParticipantAsync(int tripId, int participantUserId, int currentUserId);
        Task<ParticipantDto?> UpdateParticipantRoleAsync(int tripId, int participantUserId, UpdateParticipantRoleDto dto, int currentUserId);
        Task<List<ParticipantDto>> GetTripParticipantsAsync(int tripId, int currentUserId);
    }
}