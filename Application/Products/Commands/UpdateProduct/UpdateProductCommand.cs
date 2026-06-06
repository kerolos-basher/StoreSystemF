namespace Application.Products.Commands.UpdateProduct;

public sealed record UpdateProductCommand(long ProductId, string ProductName) : ICommand;
