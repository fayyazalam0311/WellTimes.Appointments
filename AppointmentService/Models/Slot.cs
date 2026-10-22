namespace AppointmentService.Models
{
    public class Slot
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public bool IsBooked { get; set; }
        public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    }
}
