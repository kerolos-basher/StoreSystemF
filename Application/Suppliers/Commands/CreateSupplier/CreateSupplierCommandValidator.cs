namespace Application.Suppliers.Commands.CreateSupplier;

public sealed class CreateSupplierCommandValidator : AbstractValidator<CreateSupplierCommand>
{
    public CreateSupplierCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("اسم المورد مطلوب.")
            .MaximumLength(200).WithMessage("اسم المورد طويل جداً.");
    }
}
