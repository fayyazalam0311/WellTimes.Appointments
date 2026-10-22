namespace AuthService.Exceptions
{
    public class InvalidCredentialsException : AppException
    {
        public InvalidCredentialsException() : base("Invalid Email or Password.", StatusCodes.Status401Unauthorized)
        {
        }
    }
}
