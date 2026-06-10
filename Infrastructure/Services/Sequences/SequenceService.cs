using Application.Abstractions.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Services.Sequences;

public sealed class SequenceService(IApplicationDbContext context) : ISequenceService
{
    public async Task<long> GetNextValueAsync(string sequenceName, CancellationToken cancellationToken = default)
    {
        var result = await context.Database
            .SqlQueryRaw<long>($"SELECT NEXT VALUE FOR [{sequenceName}]")
            .ToListAsync(cancellationToken);

        return result.First();
    }
}
