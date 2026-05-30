using Application.Abstractions.Persistence;
using Infrastructure.Database.Context;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Sequences;

public sealed class SequenceService(StoreContext context) : ISequenceService
{
    public async Task<long> GetNextValueAsync(
        string sequenceName,
        CancellationToken cancellationToken = default)
    {
        return await context.Database
            .SqlQuery<long>($"SELECT NEXT VALUE FOR [{sequenceName}]")
            .SingleAsync(cancellationToken);
    }
}
