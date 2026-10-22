namespace AppointmentService.Models
{
    public class Appointment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid SlotId { get; set; }
        public Slot Slot { get; set; } = null!;
        public Guid PatientId { get; set; }
        public AppointmentStatus Status { get; set; } = AppointmentStatus.Booked;
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
        public DateTime? CancelledAtUtc { get; set; }
    }
}
