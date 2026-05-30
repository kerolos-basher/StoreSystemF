using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductDetails;

public sealed record GetProductDetailsQuery(long ProductId) : IQuery<ProductDetailsDto>;
