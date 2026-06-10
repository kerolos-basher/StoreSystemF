namespace Application.Products.Commands.UpdateProductDetails;

public sealed record UpdateProductDetailsCommand(
    long ProductId,
    long ProductDetailsId,
    long? SupplierId,
    long? CategoryId,
    decimal PurchasePrice,
    decimal SellingPrice,
    int Quantity,
    string Notes) : ICommand;
