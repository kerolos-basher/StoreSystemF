namespace Domain.SupplierAggregate;

public class Supplier : ParentEntity
{
    public string Name { get; private set; } = string.Empty;

    private Supplier()
    {
    }

    private Supplier(long id, string name)
    {
        EnsureValidId(id);
        Id = id;
        Name = name;
    }

    public static Supplier Create(long id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم المورد مطلوب.");

        return new Supplier(id, name.Trim());
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
