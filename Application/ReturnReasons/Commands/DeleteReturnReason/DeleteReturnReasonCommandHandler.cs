using Application.Abstractions.Persistence;

namespace Application.ReturnReasons.Commands.DeleteReturnReason;

public sealed class DeleteReturnReasonCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteReturnReasonCommand>
{
    public async Task Handle(DeleteReturnReasonCommand request, CancellationToken cancellationToken)
    {
        var reason = await context.ReturnReason
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new Exception("سبب المرتجع غير موجود.");

        reason.SoftDelete();
        await context.SaveChangesAsync();
    }
}
