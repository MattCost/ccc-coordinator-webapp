namespace CCC.Exceptions;

public class EntityNotFoundException : Exception
{
    public EntityNotFoundException(Type entityType, int id, Exception? innerEx = null) : base($"{entityType.Name} {id} not found.", innerEx) {}
}