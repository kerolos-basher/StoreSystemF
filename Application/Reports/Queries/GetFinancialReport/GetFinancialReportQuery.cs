namespace Application.Reports.Queries.GetFinancialReport;

public sealed record GetFinancialReportQuery(DateTime? From, DateTime? To) : IQuery<FinancialReportDto>;

public sealed record FinancialReportDto(
    decimal TotalSales,
    decimal TotalPurchaseCost,
    decimal NetProfit,
    decimal CashSales,
    decimal DeferredSales,
    decimal OutstandingDebts,
    decimal InventoryValue,
    IReadOnlyList<TopProductDto> TopProducts,
    IReadOnlyList<TopCustomerDto> TopCustomers);

public sealed record TopProductDto(string ProductName, int QuantitySold, decimal Revenue);

public sealed record TopCustomerDto(string CustomerName, int InvoiceCount, decimal TotalSpent);
