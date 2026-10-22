namespace AppointmentService.Exceptions
{
    public class SlotOverlapException : AppException
    {
        public SlotOverlapException()
            : base("You already have a slot that overlaps with this time.s"
                  , StatusCodes.Status409Conflict)
        {
        }
    }
}
