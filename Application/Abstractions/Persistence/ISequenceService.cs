namespace Application.Abstractions.Persistence;

public interface ISequenceService
{
    Task<long> GetNextValueAsync(string sequenceName, CancellationToken cancellationToken = default);
}
