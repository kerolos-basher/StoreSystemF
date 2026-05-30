namespace Domain.ProductAggregate;

public sealed record StockAllocation(
    long ProductDetailsId,
    int Quantity,
    decimal UnitPrice);
