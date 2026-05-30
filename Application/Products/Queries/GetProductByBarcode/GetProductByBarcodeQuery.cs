using Application.Products.Dtos;

namespace Application.Products.Queries.GetProductByBarcode;

public sealed record GetProductByBarcodeQuery(string Barcode) : IQuery<ProductByBarcodeDto>;
