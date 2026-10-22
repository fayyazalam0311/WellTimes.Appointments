namespace AppointmentService.Exceptions
{
    public class InvalidSlotException : AppException
    {
        public InvalidSlotException(string message) 
            : base(message, StatusCodes.Status400BadRequest)
        {
        }
    }
}
