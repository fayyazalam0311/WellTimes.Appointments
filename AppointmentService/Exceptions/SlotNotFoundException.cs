namespace AppointmentService.Exceptions
{
    public class SlotNotFoundException : AppException
    {
        public SlotNotFoundException()
            : base("The requested slot was not found.", StatusCodes.Status404NotFound)
        {
        }
    }
}
