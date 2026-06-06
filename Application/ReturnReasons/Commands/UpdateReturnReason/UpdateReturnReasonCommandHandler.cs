using Application.Abstractions.Persistence;

namespace Application.ReturnReasons.Commands.UpdateReturnReason;

public sealed class UpdateReturnReasonCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateReturnReasonCommand>
{
    public async Task Handle(UpdateReturnReasonCommand request, CancellationToken cancellationToken)
    {
        var reason = await context.ReturnReason
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new Exception("سبب المرتجع غير موجود.");

        reason.Update(request.Name, request.IsReturnToStock);
        await context.SaveChangesAsync();
    }
}
