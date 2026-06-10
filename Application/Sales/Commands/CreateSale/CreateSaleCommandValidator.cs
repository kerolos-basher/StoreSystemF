namespace Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductDetailsId).GreaterThan(0).WithMessage("معرف تفاصيل المنتج مطلوب.");
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
            item.RuleFor(x => x.UnitPrice).GreaterThan(0).WithMessage("سعر الوحدة يجب أن يكون أكبر من صفر.");
        });

        RuleFor(x => x)
            .Must(x => !x.IsDeferredPayment || !string.IsNullOrWhiteSpace(x.CustomerName))
            .WithMessage("يجب تحديد اسم العميل للدفع الآجل.");
    }
}
