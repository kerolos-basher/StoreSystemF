namespace Application.Suppliers.Commands.CreateSupplier;

public sealed record CreateSupplierCommand(string Name) : ICommand<CreateSupplierResultDto>;

public sealed record CreateSupplierResultDto(string Id, string Name);
