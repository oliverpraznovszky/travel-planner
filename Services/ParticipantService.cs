using Microsoft.EntityFrameworkCore;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class ParticipantService : IParticipantService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripService _tripService;

        public ParticipantService(ApplicationDbContext context, ITripService tripService)
        {
            _context = context;
            _tripService = tripService;
        }

        public async Task<ParticipantDto?> AddParticipantAsync(int tripId, AddParticipantDto dto, int currentUserId)
        {
            // Ellenőrizzük, hogy a current user Owner-e a trip-nek
            var trip = await _context.Trips
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null || trip.CreatedById != currentUserId)
            {
                return null; // Csak Owner adhat hozzá résztvevőt
            }

            // Keressük meg a felhasználót email alapján
            var userToAdd = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (userToAdd == null)
            {
                return null; // Felhasználó nem található
            }

            // Ellenőrizzük, hogy már résztvevő-e
            var existingParticipant = await _context.TripParticipants
                .FirstOrDefaultAsync(p => p.TripId == tripId && p.UserId == userToAdd.Id);

            if (existingParticipant != null)
            {
                return null; // Már résztvevő
            }

            var participant = new TripParticipant
            {
                TripId = tripId,
                UserId = userToAdd.Id,
                Role = dto.Role,
                JoinedAt = DateTime.UtcNow
            };

            _context.TripParticipants.Add(participant);
            await _context.SaveChangesAsync();

            return new ParticipantDto
            {
                UserId = userToAdd.Id,
                Username = userToAdd.Username,
                Email = userToAdd.Email,
                Role = participant.Role,
                JoinedAt = participant.JoinedAt
            };
        }

        public async Task<bool> RemoveParticipantAsync(int tripId, int participantUserId, int currentUserId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null || trip.CreatedById != currentUserId)
            {
                return false; // Csak Owner távolíthat el résztvevőt
            }

            // Owner-t nem lehet eltávolítani
            if (participantUserId == trip.CreatedById)
            {
                return false;
            }

            var participant = await _context.TripParticipants
                .FirstOrDefaultAsync(p => p.TripId == tripId && p.UserId == participantUserId);

            if (participant == null)
            {
                return false;
            }

            _context.TripParticipants.Remove(participant);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<ParticipantDto?> UpdateParticipantRoleAsync(int tripId, int participantUserId, UpdateParticipantRoleDto dto, int currentUserId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null || trip.CreatedById != currentUserId)
            {
                return null; // Csak Owner módosíthat szerepkört
            }

            // Owner szerepét nem lehet megváltoztatni
            if (participantUserId == trip.CreatedById)
            {
                return null;
            }

            var participant = await _context.TripParticipants
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.TripId == tripId && p.UserId == participantUserId);

            if (participant == null)
            {
                return null;
            }

            participant.Role = dto.Role;
            await _context.SaveChangesAsync();

            return new ParticipantDto
            {
                UserId = participant.UserId,
                Username = participant.User.Username,
                Email = participant.User.Email,
                Role = participant.Role,
                JoinedAt = participant.JoinedAt
            };
        }

        public async Task<List<ParticipantDto>> GetTripParticipantsAsync(int tripId, int currentUserId)
        {
            if (!await _tripService.UserHasAccessToTripAsync(tripId, currentUserId))
            {
                return new List<ParticipantDto>();
            }

            var participants = await _context.TripParticipants
                .Include(p => p.User)
                .Where(p => p.TripId == tripId)
                .Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    Username = p.User.Username,
                    Email = p.User.Email,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt
                })
                .OrderBy(p => p.JoinedAt)
                .ToListAsync();

            return participants;
        }
    }
}