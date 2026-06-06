namespace Domain.CategoryAggregate;

public class Category : ParentEntity
{
    public string Name { get; private set; } = string.Empty;

    private Category()
    {
    }

    public static Category Create(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new Exception("اسم الفئة مطلوب.");

        return new Category
        {
            Name = name.Trim()
        };
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
