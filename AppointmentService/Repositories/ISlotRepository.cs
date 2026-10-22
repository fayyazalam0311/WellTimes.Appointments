using AppointmentService.Models;

namespace AppointmentService.Repositories
{
    public interface ISlotRepository
    {
        Task AddAsync(
            Slot slot, 
            CancellationToken cancellationToken = default);

        Task<bool> HasOverlapAsync(
            Guid doctorId,
            DateTime startUtc,
            DateTime endUtc,
            CancellationToken cancellationToken = default);

        Task<List<Slot>> GetAvailableAsync(
            DateTime fromUtc,
            CancellationToken cancellationToken = default);

        Task<Slot?> GetByIdAsync(
            Guid Id,
            CancellationToken cancellationToken = default);
    }
}
