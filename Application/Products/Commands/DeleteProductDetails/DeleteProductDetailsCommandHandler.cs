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
            ?? throw new Exception("المنتج غير موجود.");

        product.DeleteDetails(request.ProductDetailsId);
        await context.SaveChangesAsync();
    }
}
