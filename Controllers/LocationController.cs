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
    public class LocationController : ControllerBase
    {
        private readonly ILocationService _locationService;

        public LocationController(ILocationService locationService)
        {
            _locationService = locationService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/trips/5/location
        [HttpGet]
        public async Task<IActionResult> GetLocations(int tripId)
        {
            var userId = GetCurrentUserId();
            var locations = await _locationService.GetTripLocationsAsync(tripId, userId);
            return Ok(locations);
        }

        // GET: api/trips/5/location/10
        [HttpGet("{locationId}")]
        public async Task<IActionResult> GetLocation(int locationId)
        {
            var userId = GetCurrentUserId();
            var location = await _locationService.GetLocationByIdAsync(locationId, userId);

            if (location == null)
            {
                return NotFound(new { message = "Location not found or access denied" });
            }

            return Ok(location);
        }

        // POST: api/trips/5/location
        [HttpPost]
        public async Task<IActionResult> CreateLocation(int tripId, [FromBody] CreateLocationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var location = await _locationService.CreateLocationAsync(tripId, dto, userId);

            if (location == null)
            {
                return BadRequest(new { message = "Unable to create location. You don't have permission." });
            }

            return CreatedAtAction(nameof(GetLocation), new { tripId, locationId = location.Id }, location);
        }

        // PUT: api/trips/5/location/10
        [HttpPut("{locationId}")]
        public async Task<IActionResult> UpdateLocation(int locationId, [FromBody] UpdateLocationDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var location = await _locationService.UpdateLocationAsync(locationId, dto, userId);

            if (location == null)
            {
                return NotFound(new { message = "Location not found or you don't have permission" });
            }

            return Ok(location);
        }

        // DELETE: api/trips/5/location/10
        [HttpDelete("{locationId}")]
        public async Task<IActionResult> DeleteLocation(int locationId)
        {
            var userId = GetCurrentUserId();
            var result = await _locationService.DeleteLocationAsync(locationId, userId);

            if (!result)
            {
                return NotFound(new { message = "Location not found or you don't have permission" });
            }

            return NoContent();
        }
    }
}