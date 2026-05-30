namespace Application.Sales.Commands.CreateSale;

public sealed class CreateSaleCommandValidator : AbstractValidator<CreateSaleCommand>
{
    public CreateSaleCommandValidator()
    {
        RuleFor(x => x.Items)
            .NotEmpty().WithMessage("يجب إضافة صنف واحد على الأقل.");

        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(x => x.ProductId).NotEmpty().WithMessage("معرف المنتج مطلوب.");
            item.RuleFor(x => x.Quantity).GreaterThan(0).WithMessage("الكمية يجب أن تكون أكبر من صفر.");
        });

        RuleFor(x => x.Discount).GreaterThanOrEqualTo(0).WithMessage("الخصم لا يمكن أن يكون سالباً.");
        RuleFor(x => x.Tax).GreaterThanOrEqualTo(0).WithMessage("الضريبة لا يمكن أن تكون سالبة.");
    }
}
