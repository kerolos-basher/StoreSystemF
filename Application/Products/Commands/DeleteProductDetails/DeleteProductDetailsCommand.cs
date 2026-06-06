namespace Application.Products.Commands.DeleteProductDetails;

public sealed record DeleteProductDetailsCommand(long ProductId, long ProductDetailsId) : ICommand;
