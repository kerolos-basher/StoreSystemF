using Application.Customers.Queries.GetCustomersAutocomplete;
using Application.Sales.Queries.SearchSalesInvoicesByCustomer;
using Infrastructure.Services.LogFile;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Store_Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/v1/customers")]
public sealed class CustomersController : StoreBaseController
{
    private readonly ISender _sender;

    public CustomersController(LogFileService logger, ISender sender) : base(logger) => _sender = sender;

    [HttpGet("autocomplete")]
    public Task<IActionResult> Autocomplete([FromQuery] string q, CancellationToken cancellationToken = default) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new GetCustomersAutocompleteQuery(q ?? string.Empty), cancellationToken)));

    [HttpGet("{customerId}/invoices")]
    public Task<IActionResult> GetInvoices(long customerId, CancellationToken cancellationToken) =>
        TryCatchLogAsync(async () =>
            Ok(await _sender.Send(new SearchSalesInvoicesByCustomerQuery(customerId), cancellationToken)));
}
