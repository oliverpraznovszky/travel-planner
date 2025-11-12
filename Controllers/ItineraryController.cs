using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using travel_planner.DTOs;
using travel_planner.Services;

namespace travel_planner.Controllers
{
    [Route("api/trips/{tripId}/[controller]")]
    [ApiController]
    [Authorize]
    public class ItineraryController : ControllerBase
    {
        private readonly IItineraryService _itineraryService;

        public ItineraryController(IItineraryService itineraryService)
        {
            _itineraryService = itineraryService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/trips/5/itinerary
        [HttpGet]
        public async Task<IActionResult> GetItineraries(int tripId)
        {
            var userId = GetCurrentUserId();
            var itineraries = await _itineraryService.GetTripItinerariesAsync(tripId, userId);
            return Ok(itineraries);
        }

        // GET: api/trips/5/itinerary/10
        [HttpGet("{itineraryId}")]
        public async Task<IActionResult> GetItinerary(int itineraryId)
        {
            var userId = GetCurrentUserId();
            var itinerary = await _itineraryService.GetItineraryByIdAsync(itineraryId, userId);

            if (itinerary == null)
            {
                return NotFound(new { message = "Itinerary not found or access denied" });
            }

            return Ok(itinerary);
        }

        // POST: api/trips/5/itinerary
        [HttpPost]
        public async Task<IActionResult> CreateItinerary(int tripId, [FromBody] CreateItineraryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var itinerary = await _itineraryService.CreateItineraryAsync(tripId, dto, userId);

            if (itinerary == null)
            {
                return BadRequest(new { message = "Unable to create itinerary. Day number already exists or you don't have permission." });
            }

            return CreatedAtAction(nameof(GetItinerary), new { tripId, itineraryId = itinerary.Id }, itinerary);
        }

        // PUT: api/trips/5/itinerary/10
        [HttpPut("{itineraryId}")]
        public async Task<IActionResult> UpdateItinerary(int itineraryId, [FromBody] UpdateItineraryDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var itinerary = await _itineraryService.UpdateItineraryAsync(itineraryId, dto, userId);

            if (itinerary == null)
            {
                return NotFound(new { message = "Itinerary not found or you don't have permission" });
            }

            return Ok(itinerary);
        }

        // DELETE: api/trips/5/itinerary/10
        [HttpDelete("{itineraryId}")]
        public async Task<IActionResult> DeleteItinerary(int itineraryId)
        {
            var userId = GetCurrentUserId();
            var result = await _itineraryService.DeleteItineraryAsync(itineraryId, userId);

            if (!result)
            {
                return NotFound(new { message = "Itinerary not found or you don't have permission" });
            }

            return NoContent();
        }
    }
}