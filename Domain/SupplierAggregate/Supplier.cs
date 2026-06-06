namespace Domain.SupplierAggregate;

public class Supplier : ParentEntity
{
    public string Name { get; private set; } = string.Empty;

    private Supplier()
    {
    }

    public static Supplier Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم المورد مطلوب.");

        return new Supplier
        {
            Name = name.Trim()
        };
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم المورد مطلوب.");

        Name = name.Trim();
        MarkUpdated();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}
