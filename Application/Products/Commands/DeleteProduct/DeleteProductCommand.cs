namespace Application.Products.Commands.DeleteProduct;

public sealed record DeleteProductCommand(long ProductId) : ICommand;
