namespace Domain.CategoryAggregate;

public class Category : ParentEntity
{
    public string Name { get; private set; } = string.Empty;

    private Category()
    {
    }

    private Category(long id, string name)
    {
        EnsureValidId(id);
        Id = id;
        Name = name;
    }

    public static Category Create(long id, string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم الفئة مطلوب.");

        return new Category(id, name.Trim());
    }

    public void Update(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم الفئة مطلوب.");

        Name = name.Trim();
        MarkUpdated();
    }

    public void SoftDelete()
    {
        IsDeleted = true;
        MarkUpdated();
    }
}
