using Microsoft.EntityFrameworkCore;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class TripService : ITripService
    {
        private readonly ApplicationDbContext _context;

        public TripService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<TripDto>> GetUserTripsAsync(int userId)
        {
            var trips = await _context.Trips
                .Include(t => t.CreatedBy)
                .Include(t => t.Participants)
                .Include(t => t.Locations)
                .Where(t => t.CreatedById == userId ||
                           t.Participants.Any(p => p.UserId == userId))
                .OrderByDescending(t => t.CreatedAt)
                .Select(t => new TripDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Description = t.Description,
                    Budget = t.Budget,
                    CreatedById = t.CreatedById,
                    CreatedByUsername = t.CreatedBy.Username,
                    CreatedAt = t.CreatedAt,
                    ParticipantCount = t.Participants.Count,
                    LocationCount = t.Locations.Count
                })
                .ToListAsync();

            return trips;
        }

        public async Task<TripDetailDto?> GetTripByIdAsync(int tripId, int userId)
        {
            if (!await UserHasAccessToTripAsync(tripId, userId))
            {
                return null;
            }

            var trip = await _context.Trips
                .Include(t => t.CreatedBy)
                .Include(t => t.Participants)
                    .ThenInclude(p => p.User)
                .Include(t => t.Locations)
                .Include(t => t.Itineraries)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
            {
                return null;
            }

            return new TripDetailDto
            {
                Id = trip.Id,
                Title = trip.Title,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Description = trip.Description,
                Budget = trip.Budget,
                CreatedById = trip.CreatedById,
                CreatedByUsername = trip.CreatedBy.Username,
                CreatedAt = trip.CreatedAt,
                Participants = trip.Participants.Select(p => new ParticipantDto
                {
                    UserId = p.UserId,
                    Username = p.User.Username,
                    Email = p.User.Email,
                    Role = p.Role,
                    JoinedAt = p.JoinedAt
                }).ToList(),
                Locations = trip.Locations.Select(l => new LocationDto
                {
                    Id = l.Id,
                    Name = l.Name,
                    Address = l.Address,
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    Type = l.Type,
                    CreatedAt = l.CreatedAt
                }).ToList(),
                ItineraryCount = trip.Itineraries.Count
            };
        }

        public async Task<TripDto?> CreateTripAsync(CreateTripDto createTripDto, int userId)
        {
            // Validáció: EndDate nem lehet korábbi mint StartDate
            if (createTripDto.EndDate < createTripDto.StartDate)
            {
                return null;
            }

            var trip = new Trip
            {
                Title = createTripDto.Title,
                StartDate = createTripDto.StartDate,
                EndDate = createTripDto.EndDate,
                Description = createTripDto.Description,
                Budget = createTripDto.Budget,
                CreatedById = userId,
                CreatedAt = DateTime.UtcNow
            };

            _context.Trips.Add(trip);
            await _context.SaveChangesAsync();

            // Létrehozó automatikusan Owner lesz
            var participant = new TripParticipant
            {
                TripId = trip.Id,
                UserId = userId,
                Role = ParticipantRole.Owner,
                JoinedAt = DateTime.UtcNow
            };

            _context.TripParticipants.Add(participant);
            await _context.SaveChangesAsync();

            // User adatok betöltése
            var user = await _context.Users.FindAsync(userId);

            return new TripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Description = trip.Description,
                Budget = trip.Budget,
                CreatedById = trip.CreatedById,
                CreatedByUsername = user!.Username,
                CreatedAt = trip.CreatedAt,
                ParticipantCount = 1,
                LocationCount = 0
            };
        }

        public async Task<TripDto?> UpdateTripAsync(int tripId, UpdateTripDto updateTripDto, int userId)
        {
            if (!await UserCanEditTripAsync(tripId, userId))
            {
                return null;
            }

            var trip = await _context.Trips
                .Include(t => t.CreatedBy)
                .Include(t => t.Participants)
                .Include(t => t.Locations)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
            {
                return null;
            }

            // Validáció
            if (updateTripDto.EndDate < updateTripDto.StartDate)
            {
                return null;
            }

            trip.Title = updateTripDto.Title;
            trip.StartDate = updateTripDto.StartDate;
            trip.EndDate = updateTripDto.EndDate;
            trip.Description = updateTripDto.Description;
            trip.Budget = updateTripDto.Budget;

            await _context.SaveChangesAsync();

            return new TripDto
            {
                Id = trip.Id,
                Title = trip.Title,
                StartDate = trip.StartDate,
                EndDate = trip.EndDate,
                Description = trip.Description,
                Budget = trip.Budget,
                CreatedById = trip.CreatedById,
                CreatedByUsername = trip.CreatedBy.Username,
                CreatedAt = trip.CreatedAt,
                ParticipantCount = trip.Participants.Count,
                LocationCount = trip.Locations.Count
            };
        }

        public async Task<bool> DeleteTripAsync(int tripId, int userId)
        {
            var trip = await _context.Trips
                .FirstOrDefaultAsync(t => t.Id == tripId && t.CreatedById == userId);

            if (trip == null)
            {
                return false;
            }

            _context.Trips.Remove(trip);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<bool> UserHasAccessToTripAsync(int tripId, int userId)
        {
            return await _context.Trips
                .AnyAsync(t => t.Id == tripId &&
                              (t.CreatedById == userId ||
                               t.Participants.Any(p => p.UserId == userId)));
        }

        public async Task<bool> UserCanEditTripAsync(int tripId, int userId)
        {
            var trip = await _context.Trips
                .Include(t => t.Participants)
                .FirstOrDefaultAsync(t => t.Id == tripId);

            if (trip == null)
            {
                return false;
            }

            // Owner mindig szerkeszthet
            if (trip.CreatedById == userId)
            {
                return true;
            }

            // Editor szerepkörű résztvevő is szerkeszthet
            var participant = trip.Participants
                .FirstOrDefault(p => p.UserId == userId);

            return participant?.Role == ParticipantRole.Owner ||
                   participant?.Role == ParticipantRole.Editor;
        }
    }
}