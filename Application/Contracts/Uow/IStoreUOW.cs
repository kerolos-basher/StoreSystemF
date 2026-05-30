namespace Application.UOW;

public interface IStoreUOW
{
    Task SaveChangesAsync(long? userId = null);
    void AddToPendingActions(Action action);
    void AddToPendingActions(IEnumerable<Action> actions);
}
