namespace Application.Suppliers.Commands.UpdateSupplier;

public sealed record UpdateSupplierCommand(long Id, string Name) : ICommand;
