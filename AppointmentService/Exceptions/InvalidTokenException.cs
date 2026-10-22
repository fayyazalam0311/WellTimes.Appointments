using Microsoft.AspNetCore.Http;

namespace AppointmentService.Exceptions
{
    public class InvalidTokenException : AppException
    {
        public InvalidTokenException() 
            : base("The token does not contain a valid user id"
                  , StatusCodes.Status401Unauthorized)
        {
        }
    }
}
