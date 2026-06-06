using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Domain.ProductAggregate;
using MediatR;
using Application.Suppliers.Commands.CreateSupplier;
using Domain.InventoryAggregate;

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
        var barCode = string.IsNullOrWhiteSpace(request.Barcode) ? null : request.Barcode.Trim();
        var purchaseDate = request.PurchaseDate?.ToUniversalTime();

        var product = await FindProductAsync(productName, request.ExistingProductId, cancellationToken);
        long? productDetailsId = null;
        string generatedBarcode;

        if (product is null)
        {
            product = Product.Create(
                productName,
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                barCode,
                purchaseDate);

            context.Product.Add(product);
            await context.SaveChangesAsync();

            var details = product.ProductDetails.First();
            productDetailsId = details.Id;
            generatedBarcode = details.BarCode;

            context.InventoryTransaction.Add(
                InventoryTransaction.CreatePurchase(
                    product.Id,
                    details.Id,
                    request.Quantity,
                    $"Purchase-{product.Id}"));
        }
        else
        {
            var beforeCount = product.ProductDetails.Count;
            product.AddOrUpdateDetails(
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                barCode,
                purchaseDate);

            await context.SaveChangesAsync();

            var details = product.ProductDetails.Count > beforeCount
                ? product.ProductDetails.OrderByDescending(x => x.Id).First()
                : product.ProductDetails.First(x =>
                    x.SupplierId == supplierId &&
                    x.CategoryId == request.CategoryId &&
                    x.Price == request.PurchasePrice &&
                    x.SeLingPrice == sellingPrice);

            productDetailsId = details.Id;
            generatedBarcode = details.BarCode;

            context.InventoryTransaction.Add(
                InventoryTransaction.CreatePurchase(
                    product.Id,
                    details.Id,
                    request.Quantity,
                    $"Purchase-{product.Id}"));
        }

        await context.SaveChangesAsync();

        return new CreatePurchaseEntryResultDto(product.Id, productDetailsId, generatedBarcode);
    }

    private async Task<Product?> FindProductAsync(
        string productName,
        long? existingProductId,
        CancellationToken cancellationToken)
    {
        var query = context.Product
            .Include(p => p.ProductDetails)
            .AsQueryable();

        if (existingProductId.HasValue)
        {
            return await query.FirstOrDefaultAsync(p => p.Id == existingProductId.Value, cancellationToken);
        }

        return await query.FirstOrDefaultAsync(
            p => p.ProductName.ToLower() == productName.ToLower(),
            cancellationToken);
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
