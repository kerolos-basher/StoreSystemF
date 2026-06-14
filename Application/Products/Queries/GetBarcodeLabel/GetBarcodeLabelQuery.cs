using Application.Products.Dtos;

namespace Application.Products.Queries.GetBarcodeLabel;

public sealed record GetBarcodeLabelQuery(long ProductDetailsId) : IQuery<BarcodeLabelDto>;
