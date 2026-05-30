// Ignore Spelling: EPTIUOW StoreUOW
using Application.UOW;

namespace Infrastructure.UOW;
public class StoreUOW : IStoreUOW
{
    private readonly StoreContext Context;
    private List<Action> PendingActionsOverSaving = new List<Action>();

    public StoreUOW(StoreContext _Context)
    {
        this.Context = _Context;
    }
    public async Task SaveChangesAsync(long? userId = null)
    {
        await Context.SaveChangesAsync(userId);
        this.PendingActionsOverSaving.ForEach(s => s.Invoke());

        this.PendingActionsOverSaving = new List<Action>();
    }
    public void RemoveTracker(ParentEntity entity)
    {
        Context.Entry(entity).State = EntityState.Unchanged;
    }

    public void AddToPendingActions(Action action)
    {
        this.PendingActionsOverSaving.Add(action);
    }
    public void AddToPendingActions(IEnumerable<Action> actions)
    {
        this.PendingActionsOverSaving.AddRange(actions.ToList());
    }

}
