using Domain;

namespace Domain.SupplierAggregate;

public class Supplier : ParentEntityWithOutId
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

    public static Supplier Create(long id, string name)
    {
        if (id <= 0)
            throw new Exception("معرف المورد غير صالح.");

        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم المورد مطلوب.");

        return new Supplier
        {
            Id = id,
            Name = name.Trim()
        };
    }
}
