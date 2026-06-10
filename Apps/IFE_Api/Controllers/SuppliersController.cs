using Application.Suppliers.Commands.CreateSupplier;
using Application.Suppliers.Commands.DeleteSupplier;
using Application.Suppliers.Commands.UpdateSupplier;
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

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(
        string id,
        [FromBody] CreateSupplierRequest request,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(id, out var supplierId))
            return BadRequest(new { message = "معرف المورد غير صالح." });

        await sender.Send(new UpdateSupplierCommand(supplierId, request.Name), cancellationToken);
        return Ok();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(
        string id,
        CancellationToken cancellationToken)
    {
        if (!long.TryParse(id, out var supplierId))
            return BadRequest(new { message = "معرف المورد غير صالح." });

        await sender.Send(new DeleteSupplierCommand(supplierId), cancellationToken);
        return Ok();
    }

    public sealed class CreateSupplierRequest
    {
        public string Name { get; set; } = string.Empty;
    }
}
