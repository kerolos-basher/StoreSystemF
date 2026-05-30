using Domain;

namespace Domain.CategoryAggregate;

public class Category : ParentEntityWithOutId
{
    public string Name { get; private set; } = string.Empty;

    private Category()
    {
    }


}
