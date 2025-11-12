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
    public class ParticipantController : ControllerBase
    {
        private readonly IParticipantService _participantService;

        public ParticipantController(IParticipantService participantService)
        {
            _participantService = participantService;
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.Parse(userIdClaim!);
        }

        // GET: api/trips/5/participant
        [HttpGet]
        public async Task<IActionResult> GetParticipants(int tripId)
        {
            var userId = GetCurrentUserId();
            var participants = await _participantService.GetTripParticipantsAsync(tripId, userId);
            return Ok(participants);
        }

        // POST: api/trips/5/participant
        [HttpPost]
        public async Task<IActionResult> AddParticipant(int tripId, [FromBody] AddParticipantDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var participant = await _participantService.AddParticipantAsync(tripId, dto, userId);

            if (participant == null)
            {
                return BadRequest(new { message = "Unable to add participant. User not found, already a participant, or you don't have permission." });
            }

            return Ok(participant);
        }

        // PUT: api/trips/5/participant/3/role
        [HttpPut("{participantUserId}/role")]
        public async Task<IActionResult> UpdateParticipantRole(int tripId, int participantUserId, [FromBody] UpdateParticipantRoleDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var userId = GetCurrentUserId();
            var participant = await _participantService.UpdateParticipantRoleAsync(tripId, participantUserId, dto, userId);

            if (participant == null)
            {
                return BadRequest(new { message = "Unable to update role. Participant not found or you don't have permission." });
            }

            return Ok(participant);
        }

        // DELETE: api/trips/5/participant/3
        [HttpDelete("{participantUserId}")]
        public async Task<IActionResult> RemoveParticipant(int tripId, int participantUserId)
        {
            var userId = GetCurrentUserId();
            var result = await _participantService.RemoveParticipantAsync(tripId, participantUserId, userId);

            if (!result)
            {
                return BadRequest(new { message = "Unable to remove participant. Not found or you don't have permission." });
            }

            return NoContent();
        }
    }
}