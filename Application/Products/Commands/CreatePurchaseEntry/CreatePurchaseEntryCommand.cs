using Application.Products.Dtos;

namespace Application.Products.Commands.CreatePurchaseEntry;

public sealed record CreatePurchaseEntryCommand(
    string ProductName,
    long? ExistingProductId,
    string Barcode,
    long? CategoryId,
    string SupplierName,
    decimal PurchasePrice,
    decimal SellingPrice,
    int Quantity,
    DateTime? PurchaseDate,
    string Notes) : ICommand<CreatePurchaseEntryResultDto>;
