using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Domain.ProductAggregate;
using Domain.SupplierAggregate;
using MediatR;
using Application.Suppliers.Commands.CreateSupplier;

namespace Application.Products.Commands.CreatePurchaseEntry;

public sealed class CreatePurchaseEntryCommandHandler(
    IApplicationDbContext context,
    ISender sender)
    : ICommandHandler<CreatePurchaseEntryCommand, CreatePurchaseEntryResultDto>
{
    public async Task<CreatePurchaseEntryResultDto> Handle(
        CreatePurchaseEntryCommand request,
        CancellationToken cancellationToken)
    {
        var productName = request.ProductName.Trim();
        var sellingPrice = request.SellingPrice > 0 ? request.SellingPrice : request.PurchasePrice;
        var supplierId = await ResolveSupplierIdAsync(request.SupplierName, cancellationToken);
        var barCode = ParseBarcode(request.Barcode);
        var purchaseDate = request.PurchaseDate?.ToUniversalTime();

        var product = await FindProductAsync(productName, barCode, cancellationToken);

        if (product is null)
        {
            product = Product.Create(
                productName,
                barCode,
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                purchaseDate);

            context.Product.Add(product);
        }
        else
        {
            product.AddOrUpdateDetails(
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                purchaseDate);
        }

        await context.SaveChangesAsync();

        return new CreatePurchaseEntryResultDto(product.Id);
    }

    private async Task<Product> FindProductAsync(
        string productName,
        Guid? barCode,
        CancellationToken cancellationToken)
    {
        var query = context.Product
            .Include(p => p.ProductDetails)
            .AsQueryable();

        if (barCode.HasValue)
        {
            var byBarcode = await query.FirstOrDefaultAsync(p => p.BarCode == barCode.Value, cancellationToken);
            if (byBarcode is not null)
                return byBarcode;
        }

        return await query.FirstOrDefaultAsync(
            p => p.ProductName.ToLower() == productName.ToLower(),
            cancellationToken);
    }

    private static Guid? ParseBarcode(string barcode)
    {
        if (string.IsNullOrWhiteSpace(barcode))
            return null;

        return Guid.TryParse(barcode, out var guid) ? guid : null;
    }

    private async Task<long?> ResolveSupplierIdAsync(
        string supplierName,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(supplierName))
            return null;

        var result = await sender.Send(new CreateSupplierCommand(supplierName.Trim()), cancellationToken);
        return result.Id;
    }
}
