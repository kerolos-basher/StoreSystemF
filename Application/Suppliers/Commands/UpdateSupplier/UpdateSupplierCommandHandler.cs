using Application.Abstractions.Persistence;

namespace Application.Suppliers.Commands.UpdateSupplier;

public sealed class UpdateSupplierCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateSupplierCommand>
{
    public async Task Handle(UpdateSupplierCommand request, CancellationToken cancellationToken)
    {
        var supplier = await context.Supplier
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new Exception("المورد غير موجود.");

        supplier.Update(request.Name);
        await context.SaveChangesAsync();
    }
}
