using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Domain.LookupAggregate;

namespace Application.ReturnReasons.Commands.CreateReturnReason;

public sealed class CreateReturnReasonCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateReturnReasonCommand, ReturnReasonLookupDto>
{
    public async Task<ReturnReasonLookupDto> Handle(
        CreateReturnReasonCommand request,
        CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim().ToLower();
        var existing = await context.ReturnReason
            .FirstOrDefaultAsync(r => r.Name.ToLower() == normalized, cancellationToken);

        if (existing is not null)
            return new ReturnReasonLookupDto(existing.Id, existing.Name, existing.IsReturnToStock);

        var reason = ReturnReason.Create(request.Name.Trim(), request.IsReturnToStock);
        context.ReturnReason.Add(reason);
        await context.SaveChangesAsync();

        return new ReturnReasonLookupDto(reason.Id, reason.Name, reason.IsReturnToStock);
    }
}
