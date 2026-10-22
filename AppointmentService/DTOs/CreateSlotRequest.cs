using System.ComponentModel.DataAnnotations;

namespace AppointmentService.DTOs
{
    public class CreateSlotRequest
    {
        [Required]
        public DateTimeOffset StartTime { get; set; }
        
        [Required]
        public DateTimeOffset EndTime { get; set; }
    }
}
