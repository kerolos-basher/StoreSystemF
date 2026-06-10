using Application.Sales.Dtos;

namespace Application.Sales.Queries.GetSalesInvoiceByNumber;

public sealed record GetSalesInvoiceByNumberQuery(string InvoiceNumber) : IQuery<SalesInvoiceDto?>;
