using Application.Abstractions.Persistence;
using Application.Products.Dtos;
using Application.Suppliers.Commands.CreateSupplier;
using Domain.InventoryAggregate;
using Domain.ProductAggregate;
using MediatR;

namespace Application.Products.Commands.CreatePurchaseEntry;

public sealed class CreatePurchaseEntryCommandHandler(
    IApplicationDbContext context,
    ISender sender,
    ISequenceService sequenceService)
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
        var purchaseDate = request.PurchaseDate.HasValue
            ? request.PurchaseDate.Value.Date
            : (DateTime?)null;

        var product = await FindProductAsync(productName, request.ExistingProductId, cancellationToken);
        long productDetailsId;
        string generatedBarcode;

        if (product is null)
        {
            var productId = await sequenceService.GetNextValueAsync(SequenceKeys.ProductSequence, cancellationToken);
            productDetailsId = await sequenceService.GetNextValueAsync(SequenceKeys.ProductDetailsSequence, cancellationToken);

            product = Product.Create(
                productId,
                productDetailsId,
                productName,
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                barCode,
                purchaseDate);

            var details = product.ProductDetails.First();
            generatedBarcode = await AssignBarcodeIfNeededAsync(details, barCode, cancellationToken);

            var transactionId = await sequenceService.GetNextValueAsync(SequenceKeys.InventoryTransactionSequence, cancellationToken);
            context.Product.Add(product);
            context.InventoryTransaction.Add(
                InventoryTransaction.CreatePurchase(
                    transactionId,
                    product.Id,
                    details.Id,
                    request.Quantity,
                    $"Purchase-{product.Id}"));
        }
        else
        {
            var beforeCount = product.ProductDetails.Count;
            var newDetailsId = await sequenceService.GetNextValueAsync(SequenceKeys.ProductDetailsSequence, cancellationToken);
            product.AddOrUpdateDetails(
                newDetailsId,
                supplierId,
                request.CategoryId,
                request.PurchasePrice,
                sellingPrice,
                request.Quantity,
                request.Notes ?? string.Empty,
                barCode,
                purchaseDate);

            var details = product.ProductDetails.Count > beforeCount
                ? product.ProductDetails.First(x => x.Id == newDetailsId)
                : product.ProductDetails.First(x =>
                    !x.IsDeleted &&
                    x.SupplierId == supplierId &&
                    x.CategoryId == request.CategoryId &&
                    x.Price == request.PurchasePrice &&
                    x.SeLingPrice == sellingPrice);

            productDetailsId = details.Id;
            generatedBarcode = await AssignBarcodeIfNeededAsync(details, barCode, cancellationToken);

            var transactionId = await sequenceService.GetNextValueAsync(SequenceKeys.InventoryTransactionSequence, cancellationToken);
            context.InventoryTransaction.Add(
                InventoryTransaction.CreatePurchase(
                    transactionId,
                    product.Id,
                    details.Id,
                    request.Quantity,
                    $"Purchase-{product.Id}"));
        }

        await context.SaveChangesAsync();

        return new CreatePurchaseEntryResultDto(product.Id, productDetailsId, generatedBarcode);
    }

    private async Task<string> AssignBarcodeIfNeededAsync(
        ProductDetails details,
        string? requestedBarcode,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(requestedBarcode))
            return details.BarCode;

        if (!details.BarCode.StartsWith("TEMP", StringComparison.Ordinal))
            return details.BarCode;

        var seq = await sequenceService.GetNextValueAsync(SequenceKeys.BarCodeSequence, cancellationToken);
        details.AssignBarCode(seq);
        return details.BarCode;
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
            return await query.FirstOrDefaultAsync(p => p.Id == existingProductId.Value, cancellationToken);

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
