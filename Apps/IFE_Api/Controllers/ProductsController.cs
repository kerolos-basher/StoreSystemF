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
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/products")]
public sealed class ProductsController(ISender sender) : ControllerBase
{
    [HttpPost("purchase-entry")]
    public async Task<IActionResult> CreatePurchaseEntry(
        [FromBody] CreatePurchaseEntryRequest request,
        CancellationToken cancellationToken)
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

        var result = await sender.Send(command, cancellationToken);
        return Ok(new { productId = result.ProductId, productDetailsId = result.ProductDetailsId, barcode = result.Barcode });
    }

    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductsAutocompleteQuery(q ?? string.Empty), cancellationToken);
        return Ok(result);
    }

    [HttpGet("search-names")]
    public async Task<IActionResult> SearchNames(
        [FromQuery] string term,
        [FromQuery] int limit = 10,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new SearchProductNamesQuery(term ?? string.Empty, limit), cancellationToken);
        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] SearchProductsRequest request,
        CancellationToken cancellationToken)
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

        var result = await sender.Send(query, cancellationToken);
        return Ok(new
        {
            items = result.Items,
            totalCount = result.TotalCount,
            pageNumber = result.PageNumber,
            pageSize = result.PageSize
        });
    }

    [HttpGet("by-barcode/{barcode}")]
    public async Task<IActionResult> GetByBarcode(
        string barcode,
        CancellationToken cancellationToken)
    {
        var product = await sender.Send(new GetProductByBarcodeQuery(barcode), cancellationToken);
        return Ok(product);
    }

    [HttpGet("details/{productDetailsId}/qrcode")]
    public async Task<IActionResult> GetQrCode(
        string productDetailsId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productDetailsId, out var id))
            return BadRequest(new { message = "معرف تفاصيل المنتج غير صالح." });

        var qr = await sender.Send(new GetQRCodeQuery(id), cancellationToken);
        return Ok(qr);
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics(
        [FromQuery] int lowStockThreshold = 10,
        CancellationToken cancellationToken = default)
    {
        var stats = await sender.Send(
            new GetProductStatisticsQuery(lowStockThreshold),
            cancellationToken);

        return Ok(new
        {
            totalProducts = stats.TotalProducts,
            totalQuantity = stats.TotalQuantity,
            lowStockCount = stats.LowStockCount,
            inventoryValue = stats.InventoryValue
        });
    }

    [HttpGet("{productId}/details")]
    public async Task<IActionResult> GetDetails(
        string productId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productId, out var id))
            return BadRequest(new { message = "Invalid product id." });

        var details = await sender.Send(new GetProductDetailsQuery(id), cancellationToken);
        return Ok(details);
    }

    [HttpGet("{productId}/history")]
    public async Task<IActionResult> GetHistory(
        string productId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productId, out var id))
            return BadRequest(new { message = "Invalid product id." });

        var history = await sender.Send(new GetPurchaseHistoryQuery(id), cancellationToken);
        return Ok(history);
    }

    [HttpPut("{productId}")]
    public async Task<IActionResult> UpdateProduct(
        string productId,
        [FromBody] UpdateProductRequest request,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productId, out var id))
            return BadRequest(new { message = "معرف المنتج غير صالح." });

        await sender.Send(new UpdateProductCommand(id, request.ProductName), cancellationToken);
        return Ok();
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProduct(
        string productId,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productId, out var id))
            return BadRequest(new { message = "معرف المنتج غير صالح." });

        await sender.Send(new DeleteProductCommand(id), cancellationToken);
        return Ok();
    }

    [HttpPut("{productId}/details/{detailsId}")]
    public async Task<IActionResult> UpdateProductDetails(
        string productId,
        string detailsId,
        [FromBody] UpdateProductDetailsRequest request,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(productId, out var pid) || !long.TryParse(detailsId, out var did))
            return BadRequest(new { message = "المعرف غير صالح." });

        await sender.Send(new UpdateProductDetailsCommand(
            pid,
            did,
            request.SupplierId,
            request.CategoryId,
            request.PurchasePrice,
            request.SellingPrice,
            request.Quantity,
            request.Notes ?? string.Empty), cancellationToken);

        return Ok();
    }

    [HttpDelete("{productId}/details/{detailsId}")]
    public async Task<IActionResult> DeleteProductDetails(
        string productId,
        string detailsId,
        [FromQuery] bool forceDelete = false,
        CancellationToken cancellationToken = default)
    {
        if (!long.TryParse(productId, out var pid) || !long.TryParse(detailsId, out var did))
            return BadRequest(new { message = "المعرف غير صالح." });

        await sender.Send(new DeleteProductDetailsCommand(pid, did, forceDelete), cancellationToken);
        return Ok();
    }

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
