namespace CCC.Exceptions;

public class UserNotFoundException : Exception
{
    public UserNotFoundException(string message, Exception? innerEx = null) : base(message,innerEx) {}
}