namespace Domain.CustomerAggregate;

public sealed class Customer : ParentEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Phone { get; private set; } = string.Empty;

    private Customer()
    {
    }

    private Customer(long id, string name, string phone)
    {
        EnsureValidId(id);
        Id = id;
        Name = name;
        Phone = phone;
    }

    public static Customer Create(long id, string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم العميل مطلوب.");

        return new Customer(id, name.Trim(), phone?.Trim() ?? string.Empty);
    }

    public void Update(string name, string? phone)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم العميل مطلوب.");

        Name = name.Trim();
        Phone = phone?.Trim() ?? string.Empty;
        MarkUpdated();
    }
}
