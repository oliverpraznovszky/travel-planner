using Microsoft.EntityFrameworkCore;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class ItineraryService : IItineraryService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripService _tripService;

        public ItineraryService(ApplicationDbContext context, ITripService tripService)
        {
            _context = context;
            _tripService = tripService;
        }

        public async Task<List<ItineraryDto>> GetTripItinerariesAsync(int tripId, int userId)
        {
            if (!await _tripService.UserHasAccessToTripAsync(tripId, userId))
            {
                return new List<ItineraryDto>();
            }

            var itineraries = await _context.Itineraries
                .Include(i => i.Activities)
                    .ThenInclude(a => a.Location)
                .Where(i => i.TripId == tripId)
                .OrderBy(i => i.DayNumber)
                .Select(i => new ItineraryDto
                {
                    Id = i.Id,
                    TripId = i.TripId,
                    DayNumber = i.DayNumber,
                    Date = i.Date,
                    Notes = i.Notes,
                    Activities = i.Activities.OrderBy(a => a.OrderIndex).Select(a => new ActivityDto
                    {
                        Id = a.Id,
                        ItineraryId = a.ItineraryId,
                        LocationId = a.LocationId,
                        LocationName = a.Location != null ? a.Location.Name : null,
                        Title = a.Title,
                        Description = a.Description,
                        StartTime = a.StartTime,
                        EndTime = a.EndTime,
                        EstimatedCost = a.EstimatedCost,
                        Priority = a.Priority,
                        OrderIndex = a.OrderIndex,
                        CreatedAt = a.CreatedAt
                    }).ToList()
                })
                .ToListAsync();

            return itineraries;
        }

        public async Task<ItineraryDto?> GetItineraryByIdAsync(int itineraryId, int userId)
        {
            var itinerary = await _context.Itineraries
                .Include(i => i.Trip)
                .Include(i => i.Activities)
                    .ThenInclude(a => a.Location)
                .FirstOrDefaultAsync(i => i.Id == itineraryId);

            if (itinerary == null || !await _tripService.UserHasAccessToTripAsync(itinerary.TripId, userId))
            {
                return null;
            }

            return new ItineraryDto
            {
                Id = itinerary.Id,
                TripId = itinerary.TripId,
                DayNumber = itinerary.DayNumber,
                Date = itinerary.Date,
                Notes = itinerary.Notes,
                Activities = itinerary.Activities.OrderBy(a => a.OrderIndex).Select(a => new ActivityDto
                {
                    Id = a.Id,
                    ItineraryId = a.ItineraryId,
                    LocationId = a.LocationId,
                    LocationName = a.Location != null ? a.Location.Name : null,
                    Title = a.Title,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    EstimatedCost = a.EstimatedCost,
                    Priority = a.Priority,
                    OrderIndex = a.OrderIndex,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };
        }

        public async Task<ItineraryDto?> CreateItineraryAsync(int tripId, CreateItineraryDto dto, int userId)
        {
            if (!await _tripService.UserCanEditTripAsync(tripId, userId))
            {
                return null;
            }

            // Ellenőrizzük, hogy létezik-e már ilyen DayNumber
            var existingItinerary = await _context.Itineraries
                .FirstOrDefaultAsync(i => i.TripId == tripId && i.DayNumber == dto.DayNumber);

            if (existingItinerary != null)
            {
                return null; // Már létezik ilyen nap
            }

            var itinerary = new Itinerary
            {
                TripId = tripId,
                DayNumber = dto.DayNumber,
                Date = dto.Date,
                Notes = dto.Notes
            };

            _context.Itineraries.Add(itinerary);
            await _context.SaveChangesAsync();

            return new ItineraryDto
            {
                Id = itinerary.Id,
                TripId = itinerary.TripId,
                DayNumber = itinerary.DayNumber,
                Date = itinerary.Date,
                Notes = itinerary.Notes,
                Activities = new List<ActivityDto>()
            };
        }

        public async Task<ItineraryDto?> UpdateItineraryAsync(int itineraryId, UpdateItineraryDto dto, int userId)
        {
            var itinerary = await _context.Itineraries
                .Include(i => i.Activities)
                    .ThenInclude(a => a.Location)
                .FirstOrDefaultAsync(i => i.Id == itineraryId);

            if (itinerary == null || !await _tripService.UserCanEditTripAsync(itinerary.TripId, userId))
            {
                return null;
            }

            itinerary.Date = dto.Date;
            itinerary.Notes = dto.Notes;

            await _context.SaveChangesAsync();

            return new ItineraryDto
            {
                Id = itinerary.Id,
                TripId = itinerary.TripId,
                DayNumber = itinerary.DayNumber,
                Date = itinerary.Date,
                Notes = itinerary.Notes,
                Activities = itinerary.Activities.OrderBy(a => a.OrderIndex).Select(a => new ActivityDto
                {
                    Id = a.Id,
                    ItineraryId = a.ItineraryId,
                    LocationId = a.LocationId,
                    LocationName = a.Location != null ? a.Location.Name : null,
                    Title = a.Title,
                    Description = a.Description,
                    StartTime = a.StartTime,
                    EndTime = a.EndTime,
                    EstimatedCost = a.EstimatedCost,
                    Priority = a.Priority,
                    OrderIndex = a.OrderIndex,
                    CreatedAt = a.CreatedAt
                }).ToList()
            };
        }

        public async Task<bool> DeleteItineraryAsync(int itineraryId, int userId)
        {
            var itinerary = await _context.Itineraries
                .FirstOrDefaultAsync(i => i.Id == itineraryId);

            if (itinerary == null || !await _tripService.UserCanEditTripAsync(itinerary.TripId, userId))
            {
                return false;
            }

            _context.Itineraries.Remove(itinerary);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}