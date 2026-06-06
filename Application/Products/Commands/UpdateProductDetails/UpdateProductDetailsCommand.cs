namespace Application.Products.Commands.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand(
    long ProductId,
    long ProductDetailsId,
    long? SupplierId,
    long? CategoryId,
    decimal PurchasePrice,
    decimal SellingPrice,
    string Notes) : ICommand;
