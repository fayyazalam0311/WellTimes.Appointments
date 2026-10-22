namespace AppointmentService.Exceptions
{
    public class SlotAlreadyBookedException : AppException
    {
        public SlotAlreadyBookedException()
            : base("This slot has already been booked.", StatusCodes.Status409Conflict)
        {
        }
    }
}
