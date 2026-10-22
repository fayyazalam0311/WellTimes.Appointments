namespace AppointmentService.DTOs
{
    public class SlotResponse
    {
        public Guid Id { get; set; }
        public Guid DoctorId { get; set; }
        public string DoctorName { get; set; } = string.Empty;
        public DateTime StartTimeUtc { get; set; }
        public DateTime EndTimeUtc { get; set; }
        public bool IsBooked { get; set; }

    }
}
