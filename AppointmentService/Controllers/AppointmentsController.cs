using AppointmentService.DTOs;
using AppointmentService.Models;
using AppointmentService.Security;
using AppointmentService.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AppointmentService.Controllers
{
    [ApiController]
    [Route("api/appointments")]
    [Authorize]
    public class AppointmentsController : ControllerBase
    {
        private readonly IBookingService _bookingService;

        public AppointmentsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpPost]
        [Authorize(Roles = Roles.Patient)]
        [ProducesResponseType(
            typeof(AppointmentResponse),
            StatusCodes.Status201Created)]
        public async Task<IActionResult> Book(
            BookAppointmentRequest request,
            CancellationToken cancellationToken)
        {
            var appointment = await _bookingService.BookAsync(
                User.GetUserId(), request, cancellationToken);

            return StatusCode(StatusCodes.Status201Created, appointment);

        }

        [HttpGet("my")]
        [Authorize(Roles = Roles.Patient)]
        [ProducesResponseType(
            typeof(List<AppointmentResponse>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetMine(
            CancellationToken cancellationToken)
        {
            var appointments = await _bookingService.GetForPatientAsync(
                User.GetUserId(), cancellationToken);

            return Ok(appointments);
        }

        [HttpPost("{id:guid}/cancel")]
        [Authorize(Roles = Roles.Patient)]
        [ProducesResponseType(
            typeof(AppointmentResponse), 
            StatusCodes.Status200OK)]
        public async Task<IActionResult> Cancel(
            Guid id,
            CancellationToken cancellationToken)
        {
            var appointment = await _bookingService.CancelAsync(
                User.GetUserId(), id, cancellationToken);

            return Ok(appointment);
        }

        [HttpGet("doctor")]
        [Authorize(Roles = Roles.Doctor)]
        [ProducesResponseType(
            typeof(List<AppointmentResponse>),
            StatusCodes.Status200OK)]
        public async Task<IActionResult> GetForDoctor(
            CancellationToken cancellationToken)
        {
            var appointments = await _bookingService.GetForDoctorAsync(
                User.GetUserId(), cancellationToken);

            return Ok(appointments);
        }

    }
}
