using Application.Abstractions.Persistence;

namespace Application.Suppliers.Commands.DeleteSupplier;

public sealed class DeleteSupplierCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteSupplierCommand>
{
    public async Task Handle(DeleteSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Supplier
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new Exception("المورد غير موجود.");

        supplier.SoftDelete();
        await context.SaveChangesAsync();
    }
}
