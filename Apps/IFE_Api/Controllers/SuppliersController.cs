using Application.Suppliers.Commands.CreateSupplier;
using Application.Lookups.Queries.SearchSuppliers;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/suppliers")]
public sealed class SuppliersController(ISender sender) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Search(
        [FromQuery] string term,
        CancellationToken cancellationToken)
    {
        var suppliers = await sender.Send(new SearchSuppliersQuery(term ?? string.Empty), cancellationToken);
        return Ok(suppliers);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new CreateSupplierCommand(request.Name), cancellationToken);
        return Ok(result);
    }

    public sealed class CreateSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
