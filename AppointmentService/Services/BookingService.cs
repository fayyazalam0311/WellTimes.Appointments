using AppointmentService.DTOs;
using AppointmentService.Exceptions;
using AppointmentService.Models;
using AppointmentService.Repositories;
using Azure;


namespace AppointmentService.Services
{
    public class BookingService : IBookingService
    {
        private readonly ISlotRepository _slotRepository;
        private readonly IAppointmentRepository _appointmentRepository;
        private readonly TimeProvider _timeProvider;

        public BookingService(
            ISlotRepository slotRepository,
            IAppointmentRepository appointmentRepository,
            TimeProvider timeProvider)
        {
            _slotRepository = slotRepository;
            _appointmentRepository = appointmentRepository;
            _timeProvider = timeProvider;
        }

        public async Task<AppointmentResponse> BookAsync(
            Guid patientId,
            BookAppointmentRequest request,
            CancellationToken cancellationToken = default)
        {
            var slot = await _slotRepository
                .GetByIdAsync(request.SlotId, cancellationToken)
                ?? throw new SlotNotFoundException();

            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            if (slot.StartTimeUtc <= nowUtc)
            {
                throw new InvalidAppointmentOperationException
                    ("This slot has already started or passed.");
            }

            if (slot.IsBooked)
            {
                throw new SlotAlreadyBookedException();
            }

            var appointment = new Appointment
            {
                SlotId = slot.Id,
                PatientId = patientId
            };

            var created = await _appointmentRepository
                .TryCreateAsync(appointment, cancellationToken);

            if (!created)
            {
                throw new SlotAlreadyBookedException();
            }

            return ToResponse(appointment, slot);

        }

        public async Task<AppointmentResponse> CancelAsync(
            Guid patientId,
            Guid appointmentId,
            CancellationToken cancellationToken = default)
        {
            var appointment = await _appointmentRepository
                .GetByIdAsync(appointmentId, cancellationToken);

            if (appointment is null || appointment.PatientId != patientId)
            {
                throw new AppointmentNotFoundException();
            }

            if (appointment.Status == AppointmentStatus.Cancelled)
            {
                throw new InvalidAppointmentOperationException
                    ("This appointment is already cancelled.");
            }

            var nowUtc = _timeProvider.GetUtcNow().UtcDateTime;

            if (appointment.Slot.StartTimeUtc <= nowUtc)
            {
                throw new InvalidAppointmentOperationException
                    ("An appointment that has already started cannot be cancelled.");
            }

            appointment.Status = AppointmentStatus.Cancelled;
            appointment.CancelledAtUtc = nowUtc;
            appointment.Slot.IsBooked = false;

            await _appointmentRepository.UpdateAsync(appointment, cancellationToken);

            return ToResponse(appointment);
        }

        public async Task<List<AppointmentResponse>> GetForPatientAsync(
            Guid patientId,
            CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository
                .GetByPatientAsync(patientId, cancellationToken);
            
            return appointments.Select(ToResponse).ToList();
        }

        public async Task<List<AppointmentResponse>> GetForDoctorAsync(
            Guid doctorId,
            CancellationToken cancellationToken = default)
        {
            var appointments = await _appointmentRepository
                .GetByDoctorAsync(doctorId, cancellationToken);

            return appointments.Select(ToResponse).ToList();
        }


        private static AppointmentResponse ToResponse(Appointment appointment) 
            =>ToResponse(appointment, appointment.Slot);

        private static AppointmentResponse ToResponse(
            Appointment appointment, 
            Slot slot) => new()
        {
            Id = appointment.Id,
            SlotId = appointment.SlotId,
            PatientId = appointment.PatientId,
            DoctorId = slot.DoctorId,
            DoctorName = slot.DoctorName,
            StartTimeUtc = slot.StartTimeUtc,
            EndTimeUtc = slot.EndTimeUtc,
            Status = appointment.Status.ToString(),
            CreatedAtUtc = appointment.CreatedAtUtc,
            CancelledAtUtc = appointment.CancelledAtUtc
        };
    }
}
