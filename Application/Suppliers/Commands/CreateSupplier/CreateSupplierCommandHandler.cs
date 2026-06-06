using Application.Abstractions.Persistence;
using Domain.SupplierAggregate;

namespace Application.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandHandler(IApplicationDbContext context)
    : ICommandHandler<CreateSupplierCommand, CreateSupplierResultDto>
{
    public async Task<CreateSupplierResultDto> Handle(
        CreateSupplierCommand request,
        CancellationToken cancellationToken)
    {
        var normalized = request.Name.Trim().ToLower();
        var existing = await context.Supplier
            .FirstOrDefaultAsync(s => s.Name.ToLower() == normalized, cancellationToken);

        if (existing is not null)
            return new CreateSupplierResultDto(existing.Id, existing.Name);

        var supplier = Supplier.Create(request.Name.Trim());
        context.Supplier.Add(supplier);
        await context.SaveChangesAsync();

        return new CreateSupplierResultDto(supplier.Id, supplier.Name);
    }
}
