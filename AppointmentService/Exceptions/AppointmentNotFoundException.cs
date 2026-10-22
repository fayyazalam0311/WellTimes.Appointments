namespace AppointmentService.Exceptions
{
    public class AppointmentNotFoundException : AppException
    {
        public AppointmentNotFoundException()
            : base("The requested appointment was not found.", StatusCodes.Status404NotFound)
        {
        }
    }
}
