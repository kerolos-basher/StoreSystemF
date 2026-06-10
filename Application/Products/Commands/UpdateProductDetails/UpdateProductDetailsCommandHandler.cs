using Application.Abstractions.Persistence;

namespace Application.Products.Commands.UpdateProductDetails;

public sealed class UpdateProductDetailsCommandHandler(IApplicationDbContext context)
    : ICommandHandler<UpdateProductDetailsCommand>
{
    public async Task Handle(UpdateProductDetailsCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Product
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new Exception("المنتج غير موجود.");

        var details = product.ProductDetails.FirstOrDefault(x => x.Id == request.ProductDetailsId)
            ?? throw new Exception("التفاصيل غير موجودة.");

        details.Update(
            request.SupplierId,
            request.CategoryId,
            request.PurchasePrice,
            request.SellingPrice,
            request.Notes ?? string.Empty);

        if (request.Quantity > 0 && request.Quantity != details.Quantity)
            details.UpdateQuantity(request.Quantity);

        await context.SaveChangesAsync();
    }
}
