using AppointmentService.DTOs;

namespace AppointmentService.Services
{
    public interface ISlotService
    {
        Task<SlotResponse> CreateAsync(
            Guid doctorId,
            string doctorName,
            CreateSlotRequest request,
            CancellationToken cancellationToken = default);

        Task<List<SlotResponse>> GetAvailableAsync(
            CancellationToken cancellationToken = default);

    }
}
