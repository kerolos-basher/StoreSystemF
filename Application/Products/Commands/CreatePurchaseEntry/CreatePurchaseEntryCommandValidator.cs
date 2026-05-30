namespace Application.Products.Commands.CreatePurchaseEntry;

public sealed class CreatePurchaseEntryCommandValidator : AbstractValidator<CreatePurchaseEntryCommand>
{
    public CreatePurchaseEntryCommandValidator()
    {
        RuleFor(x => x.ProductName).NotEmpty().WithMessage("اسم المنتج مطلوب.").MaximumLength(200);
        RuleFor(x => x.PurchasePrice).GreaterThan(0).WithMessage("سعر الشراء يجب أن يكون أكبر من صفر.");
        RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
        RuleFor(x => x.SellingPrice).GreaterThanOrEqualTo(0).WithMessage("سعر البيع غير صالح.");
    }
}
