using AppointmentService.DTOs;

namespace AppointmentService.Services
{
    public interface IBookingService
    {
        Task<AppointmentResponse> BookAsync(
            Guid patientId, 
            BookAppointmentRequest request, 
            CancellationToken cancellationToken = default);

        Task<AppointmentResponse> CancelAsync(
            Guid patientId, 
            Guid appointmentId, 
            CancellationToken cancellationToken = default);

        Task<List<AppointmentResponse>> GetForPatientAsync(
            Guid patientId, 
            CancellationToken cancellationToken = default);

        Task<List<AppointmentResponse>> GetForDoctorAsync(
            Guid doctorId, 
            CancellationToken cancellationToken = default);

    }
}
