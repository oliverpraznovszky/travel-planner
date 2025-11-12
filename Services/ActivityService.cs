using Microsoft.EntityFrameworkCore;
using travel_planner.Data;
using travel_planner.DTOs;
using travel_planner.Models;

namespace travel_planner.Services
{
    public class ActivityService : IActivityService
    {
        private readonly ApplicationDbContext _context;
        private readonly ITripService _tripService;

        public ActivityService(ApplicationDbContext context, ITripService tripService)
        {
            _context = context;
            _tripService = tripService;
        }

        public async Task<ActivityDto?> GetActivityByIdAsync(int activityId, int userId)
        {
            var activity = await _context.Activities
                .Include(a => a.Itinerary)
                    .ThenInclude(i => i.Trip)
                .Include(a => a.Location)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            if (activity == null || !await _tripService.UserHasAccessToTripAsync(activity.Itinerary.TripId, userId))
            {
                return null;
            }

            return new ActivityDto
            {
                Id = activity.Id,
                ItineraryId = activity.ItineraryId,
                LocationId = activity.LocationId,
                LocationName = activity.Location?.Name,
                Title = activity.Title,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                EstimatedCost = activity.EstimatedCost,
                Priority = activity.Priority,
                OrderIndex = activity.OrderIndex,
                CreatedAt = activity.CreatedAt
            };
        }

        public async Task<ActivityDto?> CreateActivityAsync(int itineraryId, CreateActivityDto dto, int userId)
        {
            var itinerary = await _context.Itineraries
                .Include(i => i.Trip)
                .FirstOrDefaultAsync(i => i.Id == itineraryId);

            if (itinerary == null || !await _tripService.UserCanEditTripAsync(itinerary.TripId, userId))
            {
                return null;
            }

            // Ha LocationId meg van adva, ellenőrizzük hogy létezik-e és ugyanahhoz a Trip-hez tartozik-e
            if (dto.LocationId.HasValue)
            {
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == dto.LocationId.Value && l.TripId == itinerary.TripId);

                if (location == null)
                {
                    return null; // Location nem található vagy nem ehhez a Trip-hez tartozik
                }
            }

            var activity = new Activity
            {
                ItineraryId = itineraryId,
                LocationId = dto.LocationId,
                Title = dto.Title,
                Description = dto.Description,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                EstimatedCost = dto.EstimatedCost,
                Priority = dto.Priority,
                OrderIndex = dto.OrderIndex,
                CreatedAt = DateTime.UtcNow
            };

            _context.Activities.Add(activity);
            await _context.SaveChangesAsync();

            // Location név lekérése ha van
            string? locationName = null;
            if (dto.LocationId.HasValue)
            {
                var location = await _context.Locations.FindAsync(dto.LocationId.Value);
                locationName = location?.Name;
            }

            return new ActivityDto
            {
                Id = activity.Id,
                ItineraryId = activity.ItineraryId,
                LocationId = activity.LocationId,
                LocationName = locationName,
                Title = activity.Title,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                EstimatedCost = activity.EstimatedCost,
                Priority = activity.Priority,
                OrderIndex = activity.OrderIndex,
                CreatedAt = activity.CreatedAt
            };
        }

        public async Task<ActivityDto?> UpdateActivityAsync(int activityId, UpdateActivityDto dto, int userId)
        {
            var activity = await _context.Activities
                .Include(a => a.Itinerary)
                    .ThenInclude(i => i.Trip)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            if (activity == null || !await _tripService.UserCanEditTripAsync(activity.Itinerary.TripId, userId))
            {
                return null;
            }

            // Ha LocationId meg van adva, ellenőrizzük hogy létezik-e és ugyanahhoz a Trip-hez tartozik-e
            if (dto.LocationId.HasValue)
            {
                var location = await _context.Locations
                    .FirstOrDefaultAsync(l => l.Id == dto.LocationId.Value && l.TripId == activity.Itinerary.TripId);

                if (location == null)
                {
                    return null;
                }
            }

            activity.LocationId = dto.LocationId;
            activity.Title = dto.Title;
            activity.Description = dto.Description;
            activity.StartTime = dto.StartTime;
            activity.EndTime = dto.EndTime;
            activity.EstimatedCost = dto.EstimatedCost;
            activity.Priority = dto.Priority;
            activity.OrderIndex = dto.OrderIndex;

            await _context.SaveChangesAsync();

            // Location név lekérése ha van
            string? locationName = null;
            if (dto.LocationId.HasValue)
            {
                var location = await _context.Locations.FindAsync(dto.LocationId.Value);
                locationName = location?.Name;
            }

            return new ActivityDto
            {
                Id = activity.Id,
                ItineraryId = activity.ItineraryId,
                LocationId = activity.LocationId,
                LocationName = locationName,
                Title = activity.Title,
                Description = activity.Description,
                StartTime = activity.StartTime,
                EndTime = activity.EndTime,
                EstimatedCost = activity.EstimatedCost,
                Priority = activity.Priority,
                OrderIndex = activity.OrderIndex,
                CreatedAt = activity.CreatedAt
            };
        }

        public async Task<bool> DeleteActivityAsync(int activityId, int userId)
        {
            var activity = await _context.Activities
                .Include(a => a.Itinerary)
                .FirstOrDefaultAsync(a => a.Id == activityId);

            if (activity == null || !await _tripService.UserCanEditTripAsync(activity.Itinerary.TripId, userId))
            {
                return false;
            }

            _context.Activities.Remove(activity);
            await _context.SaveChangesAsync();

            return true;
        }
    }
}