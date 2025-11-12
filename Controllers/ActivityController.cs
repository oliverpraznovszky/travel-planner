using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using travel_planner.DTOs;
using travel_planner.Services;

namespace travel_planner.Controllers
{
    [Route("api/itinerary/{itineraryId}/[controller]")]
    [ApiController]
    [Authorize]
    public class ActivityController : ControllerBase
    {
        private readonly IActivityService _activityService;

        public ActivityController(IActivityService activityService)
        {
            _activityService = activityService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/itinerary/10/activity/15
        [HttpGet("{activityId}")]
        public async Task<IActionResult> GetActivity(int activityId)
        {
            var userId = GetCurrentUserId();
            var activity = await _activityService.GetActivityByIdAsync(activityId, userId);

            if (activity == null)
            {
                return NotFound(new { message = "Activity not found or access denied" });
            }

            return Ok(activity);
        }

        // POST: api/itinerary/10/activity
        [HttpPost]
        public async Task<IActionResult> CreateActivity(int itineraryId, [FromBody] CreateActivityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var activity = await _activityService.CreateActivityAsync(itineraryId, dto, userId);

            if (activity == null)
            {
                return BadRequest(new { message = "Unable to create activity. Invalid data or you don't have permission." });
            }

            return CreatedAtAction(nameof(GetActivity), new { itineraryId, activityId = activity.Id }, activity);
        }

        // PUT: api/itinerary/10/activity/15
        [HttpPut("{activityId}")]
        public async Task<IActionResult> UpdateActivity(int activityId, [FromBody] UpdateActivityDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var activity = await _activityService.UpdateActivityAsync(activityId, dto, userId);

            if (activity == null)
            {
                return NotFound(new { message = "Activity not found or you don't have permission" });
            }

            return Ok(activity);
        }

        // DELETE: api/itinerary/10/activity/15
        [HttpDelete("{activityId}")]
        public async Task<IActionResult> DeleteActivity(int activityId)
        {
            var userId = GetCurrentUserId();
            var result = await _activityService.DeleteActivityAsync(activityId, userId);

            if (!result)
            {
                return NotFound(new { message = "Activity not found or you don't have permission" });
            }

            return NoContent();
        }
    }
}