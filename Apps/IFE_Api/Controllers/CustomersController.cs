using Application.Customers.Queries.GetCustomersAutocomplete;
using Application.Sales.Queries.SearchSalesInvoicesByCustomer;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/customers")]
public sealed class CustomersController(ISender sender) : ControllerBase
{
    [HttpGet("autocomplete")]
    public async Task<IActionResult> Autocomplete(
        [FromQuery] string q,
        CancellationToken cancellationToken = default)
    {
        var result = await sender.Send(new GetCustomersAutocompleteQuery(q ?? string.Empty), cancellationToken);
        return Ok(result);
    }

    [HttpGet("{customerId}/invoices")]
    public async Task<IActionResult> GetInvoices(
        long customerId,
        CancellationToken cancellationToken)
    {
        var result = await sender.Send(new SearchSalesInvoicesByCustomerQuery(customerId), cancellationToken);
        return Ok(result);
    }
}
