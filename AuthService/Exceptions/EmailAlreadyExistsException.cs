namespace AuthService.Exceptions
{
    public class EmailAlreadyExistsException : AppException
    {
        public EmailAlreadyExistsException(string email) 
            : base($"Email '{email}' already exists.", StatusCodes.Status409Conflict) 
        { 
        }
    }
}
