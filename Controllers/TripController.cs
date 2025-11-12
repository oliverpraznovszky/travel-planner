using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using travel_planner.DTOs;
using travel_planner.Services;

namespace travel_planner.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TripController : ControllerBase
    {
        private readonly ITripService _tripService;

        public TripController(ITripService tripService)
        {
            _tripService = tripService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/Trip
        [HttpGet]
        public async Task<IActionResult> GetUserTrips()
        {
            var userId = GetCurrentUserId();
            var trips = await _tripService.GetUserTripsAsync(userId);
            return Ok(trips);
        }

        // GET: api/Trip/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTripById(int id)
        {
            var userId = GetCurrentUserId();
            var trip = await _tripService.GetTripByIdAsync(id, userId);

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found or access denied" });
            }

            return Ok(trip);
        }

        // POST: api/Trip
        [HttpPost]
        public async Task<IActionResult> CreateTrip([FromBody] CreateTripDto createTripDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var trip = await _tripService.CreateTripAsync(createTripDto, userId);

            if (trip == null)
            {
                return BadRequest(new { message = "Invalid trip data. End date must be after start date." });
            }

            return CreatedAtAction(nameof(GetTripById), new { id = trip.Id }, trip);
        }

        // PUT: api/Trip/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTrip(int id, [FromBody] UpdateTripDto updateTripDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var trip = await _tripService.UpdateTripAsync(id, updateTripDto, userId);

            if (trip == null)
            {
                return NotFound(new { message = "Trip not found, access denied, or invalid data" });
            }

            return Ok(trip);
        }

        // DELETE: api/Trip/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTrip(int id)
        {
            var userId = GetCurrentUserId();
            var result = await _tripService.DeleteTripAsync(id, userId);

            if (!result)
            {
                return NotFound(new { message = "Trip not found or you don't have permission to delete it" });
            }

            return NoContent();
        }
    }
}