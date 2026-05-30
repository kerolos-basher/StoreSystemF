using Application.Sales.Dtos;

namespace Application.Sales.Queries.GetSalesInvoice;

public sealed record GetSalesInvoiceQuery(long InvoiceId) : IQuery<SalesInvoiceDto>;
