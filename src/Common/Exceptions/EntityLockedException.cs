namespace CCC.Exceptions;

public class EntityLockedException: Exception
{
    public EntityLockedException(string message, Exception? innerEx = null) : base(message, innerEx) {}
}