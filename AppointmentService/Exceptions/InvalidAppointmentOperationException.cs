namespace AppointmentService.Exceptions
{
    public class InvalidAppointmentOperationException : AppException
    {
        public InvalidAppointmentOperationException(string message)
            : base(message, StatusCodes.Status409Conflict)
        {
        }
    }
}
