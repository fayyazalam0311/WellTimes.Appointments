using AppointmentService.Models;

namespace AppointmentService.Repositories
{
    public interface IAppointmentRepository
    {
        Task<bool> TryCreateAsync(
            Appointment appointment, 
            CancellationToken cancellationToken = default);
        Task<Appointment?> GetByIdAsync(
            Guid id, 
            CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByPatientAsync(
            Guid patientId, 
            CancellationToken cancellationToken = default);
        Task<List<Appointment>> GetByDoctorAsync(
            Guid doctorId, 
            CancellationToken cancellationToken = default);
        Task UpdateAsync(
            Appointment appointment, 
            CancellationToken cancellationToken = default);
    }
}
