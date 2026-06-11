using Application.Products.Commands.CreatePurchaseEntry;
using Application.Products.Commands.DeleteProduct;
using Application.Products.Commands.DeleteProductDetails;
using Application.Products.Commands.UpdateProduct;
using Application.Products.Commands.UpdateProductDetails;
using Application.Products.Queries.GetProductByBarcode;
using Application.Products.Queries.GetProductStatistics;
using Application.Products.Queries.GetProductDetails;
using Application.Products.Queries.GetPurchaseHistory;
using Application.Products.Queries.GetQRCode;
using Application.Products.Queries.GetProductsAutocomplete;
using Application.Products.Queries.SearchProductNames;
using Application.Products.Queries.SearchProducts;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/products")]
public sealed class ProductsController : StoreBaseController
{
    private readonly ISender _sender;

    public ProductsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpPost("purchase-entry")]
    public Task<IActionResult> CreatePurchaseEntry(
        [FromBody] CreatePurchaseEntryRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var command = new CreatePurchaseEntryCommand(
                request.ProductName,
                request.ExistingProductId,
                request.Barcode ?? string.Empty,
                request.CategoryId,
                request.SupplierName ?? string.Empty,
                request.PurchasePrice,
                request.SellingPrice,
                request.Quantity,
                request.PurchaseDate,
                request.Notes ?? string.Empty);

            var result = await _sender.Send(command, cancellationToken);
            return Ok(new { productId = result.ProductId, productDetailsId = result.ProductDetailsId, barcode = result.Barcode });
        });

    [HttpGet("autocomplete")]
    public Task<IActionResult> Autocomplete([FromQuery] string q, CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new GetProductsAutocompleteQuery(q ?? string.Empty), cancellationToken)));

    [HttpGet("search-names")]
    public Task<IActionResult> SearchNames(
        [FromQuery] string term,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new SearchProductNamesQuery(term ?? string.Empty, limit), cancellationToken)));

    [HttpGet]
    public Task<IActionResult> Search([FromQuery] SearchProductsRequest request, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var query = new SearchProductsQuery(
                request.ProductName ?? string.Empty,
                request.Barcode ?? string.Empty,
                ParseLong(request.SupplierId),
                ParseLong(request.CategoryId),
                request.PurchasePriceFrom,
                request.PurchasePriceTo,
                request.SellingPriceFrom,
                request.SellingPriceTo,
                request.QuantityFrom,
                request.QuantityTo,
                request.SortBy ?? string.Empty,
                request.SortDirection ?? "desc",
                request.PageNumber ?? 1,
                request.PageSize ?? 10);

            var result = await _sender.Send(query, cancellationToken);
            return Ok(new
            {
                items = result.Items,
                totalCount = result.TotalCount,
                pageNumber = result.PageNumber,
                pageSize = result.PageSize
            });
        });

    [HttpGet("by-barcode/{barcode}")]
    public Task<IActionResult> GetByBarcode(string barcode, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () => Ok(await _sender.Send(new GetProductByBarcodeQuery(barcode), cancellationToken)));

    [HttpGet("details/{productDetailsId}/qrcode")]
    public Task<IActionResult> GetQrCode(string productDetailsId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productDetailsId, out var id))
                return BadRequest(new { message = "معرف تفاصيل المنتج غير صالح." });

            var qr = await _sender.Send(new GetQRCodeQuery(id), cancellationToken);
            return Ok(qr);
        });

    [HttpGet("statistics")]
    public Task<IActionResult> GetStatistics(
        [FromQuery] int lowStockThreshold = 10,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
        {
            var stats = await _sender.Send(new GetProductStatisticsQuery(lowStockThreshold), cancellationToken);
            return Ok(new
            {
                totalProducts = stats.TotalProducts,
                totalQuantity = stats.TotalQuantity,
                lowStockCount = stats.LowStockCount,
                inventoryValue = stats.InventoryValue
            });
        });

    [HttpGet("{productId}/details")]
    public Task<IActionResult> GetDetails(string productId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var id))
                return BadRequest(new { message = "Invalid product id." });

            var details = await _sender.Send(new GetProductDetailsQuery(id), cancellationToken);
            return Ok(details);
        });

    [HttpGet("{productId}/history")]
    public Task<IActionResult> GetHistory(string productId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var id))
                return BadRequest(new { message = "Invalid product id." });

            var history = await _sender.Send(new GetPurchaseHistoryQuery(id), cancellationToken);
            return Ok(history);
        });

    [HttpPut("{productId}")]
    public Task<IActionResult> UpdateProduct(
        string productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var id))
                return BadRequest(new { message = "معرف المنتج غير صالح." });

            await _sender.Send(new UpdateProductCommand(id, request.ProductName), cancellationToken);
            return Ok();
        });

    [HttpDelete("{productId}")]
    public Task<IActionResult> DeleteProduct(string productId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var id))
                return BadRequest(new { message = "معرف المنتج غير صالح." });

            await _sender.Send(new DeleteProductCommand(id), cancellationToken);
            return Ok();
        });

    [HttpPut("{productId}/details/{detailsId}")]
    public Task<IActionResult> UpdateProductDetails(
        string productId,
        string detailsId,
        [FromBody] UpdateProductDetailsRequest request,
        CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var pid) || !long.TryParse(detailsId, out var did))
                return BadRequest(new { message = "المعرف غير صالح." });

            await _sender.Send(new UpdateProductDetailsCommand(
                pid,
                did,
                request.SupplierId,
                request.CategoryId,
                request.PurchasePrice,
                request.SellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty), cancellationToken);

            return Ok();
        });

    [HttpDelete("{productId}/details/{detailsId}")]
    public Task<IActionResult> DeleteProductDetails(
        string productId,
        string detailsId,
        [FromQuery] bool forceDelete = false,
        CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
        {
            if (!long.TryParse(productId, out var pid) || !long.TryParse(detailsId, out var did))
                return BadRequest(new { message = "المعرف غير صالح." });

            await _sender.Send(new DeleteProductDetailsCommand(pid, did, forceDelete), cancellationToken);
            return Ok();
        });

    private static long? ParseLong(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        return long.TryParse(value, out var id) ? id : null;
    }

    public sealed class CreatePurchaseEntryRequest
    {
        public string ProductName { get; set; } = string.Empty;
        public long? ExistingProductId { get; set; }
        public string Barcode { get; set; }
        public long? CategoryId { get; set; }
        public string SupplierName { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public DateTime? PurchaseDate { get; set; }
        public string Notes { get; set; }
    }

    public sealed class UpdateProductRequest
    {
        public string ProductName { get; set; } = string.Empty;
    }

    public sealed class UpdateProductDetailsRequest
    {
        public long? SupplierId { get; set; }
        public long? CategoryId { get; set; }
        public decimal PurchasePrice { get; set; }
        public decimal SellingPrice { get; set; }
        public int Quantity { get; set; }
        public string Notes { get; set; }
    }

    public sealed class SearchProductsRequest
    {
        public string ProductName { get; set; }
        public string Barcode { get; set; }
        public string SupplierId { get; set; }
        public string CategoryId { get; set; }
        public decimal? PurchasePriceFrom { get; set; }
        public decimal? PurchasePriceTo { get; set; }
        public decimal? SellingPriceFrom { get; set; }
        public decimal? SellingPriceTo { get; set; }
        public int? QuantityFrom { get; set; }
        public int? QuantityTo { get; set; }
        public string SortBy { get; set; }
        public string SortDirection { get; set; }
        public int? PageNumber { get; set; }
        public int? PageSize { get; set; }
    }
}
