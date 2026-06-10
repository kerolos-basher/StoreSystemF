using Application.Products.Queries.GetProductDetailsAutocomplete;
using Application.Products.Queries.SearchProductDetailsByBarcode;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/product-details")]
public sealed class ProductDetailsController(ISender sender) : ControllerBase
{
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetProductDetailsAutocompleteQuery(q ?? string.Empty), cancellationToken);
        return Ok(result);
    }

    [HttpGet("search-by-barcode/{barcode}")]
    public async Task<IActionResult> SearchByBarcode(
        string barcode,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchProductDetailsByBarcodeQuery(barcode), cancellationToken);
        if (result is null)
            return NotFound(new { message = "لم يتم العثور على المنتج." });

        return Ok(result);
    }
}
