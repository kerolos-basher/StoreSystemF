using Application.Products.Queries.GetProductDetailsAutocomplete;
using Application.Products.Queries.SearchProductDetailsByBarcode;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/product-details")]
public sealed class ProductDetailsController : StoreBaseController
{
    private readonly ISender _sender;

    public ProductDetailsController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet("autocomplete")]
    public Task<IActionResult> Autocomplete([FromQuery] string q, CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new GetProductDetailsAutocompleteQuery(q ?? string.Empty), cancellationToken)));

    [HttpGet("search-by-barcode/{barcode}")]
    public Task<IActionResult> SearchByBarcode(string barcode, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
        {
            var result = await _sender.Send(new SearchProductDetailsByBarcodeQuery(barcode), cancellationToken);
            if (result is null)
                return NotFound(new { message = "لم يتم العثور على المنتج." });

            return Ok(result);
        });
}
