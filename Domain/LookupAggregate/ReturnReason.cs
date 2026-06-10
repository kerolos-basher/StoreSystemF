namespace Domain.LookupAggregate;

public sealed class ReturnReason
{
    public int Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public bool IsReturnToStock { get; private set; }
    public bool IsDeleted { get; private set; }

    private ReturnReason()
    {
    }

    private ReturnReason(int id, string name, bool isReturnToStock)
    {
        if (id <= 0)
            throw new ArgumentException("المعرف غير صالح.", nameof(id));

        Id = id;
        Name = name;
        IsReturnToStock = isReturnToStock;
    }

    public static ReturnReason Create(int id, string name, bool isReturnToStock = true)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم السبب مطلوب.");

        return new ReturnReason(id, name.Trim(), isReturnToStock);
    }

    public void Update(string name, bool isReturnToStock)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم السبب مطلوب.");

        Name = name.Trim();
        IsReturnToStock = isReturnToStock;
    }

    public void SoftDelete()
    {
        IsDeleted = true;
    }
}
