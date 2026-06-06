using Application.Products.Dtos;

namespace Application.ReturnReasons.Queries.GetReturnReasons;

public sealed record GetReturnReasonsQuery : IQuery<IReadOnlyList<ReturnReasonLookupDto>>;
