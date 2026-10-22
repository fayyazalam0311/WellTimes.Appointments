using AppointmentService.DTOs;
using AppointmentService.Models;
using AppointmentService.Security;
using AppointmentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.Controllers
{
    [ApiController]
    [Route("api/slots")]
    [Authorize]
    public class SlotsController : ControllerBase
    {
        private readonly ISlotService _slotService;

        public SlotsController(ISlotService slotService)
        {
            _slotService = slotService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Doctor)]
        [ProducesResponseType(
            typeof(SlotResponse), 
            StatusCodes.Status201Created)]
        public async Task<IActionResult> Create(
            CreateSlotRequest request, 
            CancellationToken cancellationToken)
        {
            var slot = await _slotService.CreateAsync(
                User.GetUserId(), 
                User.GetUserName(), 
                request, 
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, slot);
        }

        [HttpGet("available")]
        [ProducesResponseType(
            typeof(List<SlotResponse>), 
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAvailable(
            CancellationToken cancellationToken)
        {
            var slots = await _slotService.GetAvailableAsync(cancellationToken);
            return Ok(slots);
        }
    }
}
