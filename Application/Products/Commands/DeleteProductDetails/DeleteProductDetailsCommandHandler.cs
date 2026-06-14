using Application.Abstractions.Persistence;

namespace Application.Products.Commands.DeleteProductDetails;

public sealed class DeleteProductDetailsCommandHandler(IApplicationDbContext context)
    : ICommandHandler<DeleteProductDetailsCommand>
{
    public async Task Handle(DeleteProductDetailsCommand request, CancellationToken cancellationToken)
    {
        var product = await context.Product
            .Include(p => p.ProductDetails)
            .FirstOrDefaultAsync(p => p.Id == request.ProductId, cancellationToken)
            ?? throw new StoreException("المنتج غير موجود.");

        var hasSales = await context.SalesInvoiceItem
            .AsNoTracking()
            .AnyAsync(x => x.ProductDetailsId == request.ProductDetailsId && !x.IsDeleted, cancellationToken);

        if (hasSales)
            throw new StoreException("لا يمكن حذف هذه الدفعة — مرتبطة بفاتورة مبيعات.");

        product.DeleteDetails(request.ProductDetailsId, request.ForceDelete);
        await context.SaveChangesAsync();
    }
}
