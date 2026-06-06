using Application.Returns.Dtos;

namespace Application.Returns.Queries.GetReturnInvoice;

public sealed record GetReturnInvoiceQuery(long ReturnInvoiceId) : IQuery<ReturnInvoiceDto>;
