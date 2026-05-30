using Application.Products.Dtos;

namespace Application.Products.Queries.GetQRCode;

public sealed record GetQRCodeQuery(long ProductId) : IQuery<QRCodeDto>;
