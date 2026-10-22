using AppointmentService.DTOs;
using AppointmentService.Exceptions;
using AppointmentService.Models;
using AppointmentService.Repositories;
using Azure;

namespace AppointmentService.Services
{
    public class SlotService : ISlotService
    {
        private readonly ISlotRepository _slotRepository;
        private readonly TimeProvider _timeProvider;

        public SlotService(ISlotRepository slotRepository, TimeProvider timeProvider)
        {
            _slotRepository = slotRepository;
            _timeProvider = timeProvider;
        }

        public async Task<SlotResponse> CreateAsync(
            Guid doctorId,
            string doctorName,
            CreateSlotRequest request,
            CancellationToken cancellationToken = default)
        {
            var startUtc = request.StartTime.UtcDateTime;
            var endUtc = request.EndTime.UtcDateTime;
            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            if (endUtc <= startUtc)
            {
                throw new InvalidSlotException("End time must be after start time.");
            }

            if (startUtc <= nowUtc)
            {
                throw new InvalidSlotException("A slot must start in the future.");
            }

            if (await _slotRepository.HasOverlapAsync(doctorId, startUtc, endUtc, cancellationToken))
            {
                throw new SlotOverlapException();
            }

            var slot = new Slot
            {
                DoctorId = doctorId,
                DoctorName = doctorName,
                StartTimeUtc = startUtc,
                EndTimeUtc = endUtc
            };

            await _slotRepository.AddAsync(slot, cancellationToken);

            return ToResponse(slot);

        }

        public async Task<List<SlotResponse>> GetAvailableAsync(
            CancellationToken cancellationToken = default)
        {
            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;
            var slots = await _slotRepository.GetAvailableAsync(nowUtc, cancellationToken);

            return slots.Select(ToResponse).ToList();
        }

        private static SlotResponse ToResponse(Slot slot) => new()
        {
            Id = slot.Id,
            DoctorId = slot.DoctorId,
            DoctorName = slot.DoctorName,
            StartTimeUtc = slot.StartTimeUtc,
            EndTimeUtc = slot.EndTimeUtc,
            IsBooked = slot.IsBooked
        };
    }
}
