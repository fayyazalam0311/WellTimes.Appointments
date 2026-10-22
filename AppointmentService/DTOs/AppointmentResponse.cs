namespace AppointmentService.DTOs
{
    public class AppointmentResponse
    {
        public Guid Id { get; set; }

        public Guid SlotId { get; set; }

        public Guid PatientId { get; set; }

        public Guid DoctorId { get; set; }

        public string DoctorName { get; set; } = string.Empty;

        public DateTime StartTimeUtc { get; set; }

        public DateTime EndTimeUtc { get; set; }

        public string Status { get; set; } = string.Empty;

        public DateTime CreatedAtUtc { get; set; }

        public DateTime? CancelledAtUtc { get; set; }
    }
}
