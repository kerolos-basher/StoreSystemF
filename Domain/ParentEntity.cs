
namespace Domain;

public abstract class ParentEntity : ISoftDelete
{
    public long Id { get; protected set; }
    public long CreatedBy { get; protected set; } = 0;
    public long? UpdatedBy { get; protected set; } = 0;
    public bool IsDeleted { get; protected set; } = false;

    public DateTime CreationTime { get; protected set; } = DateTime.Now;
    public DateTime? UpdateTime { get; protected set; }



    protected void MarkUpdated(long? updatedBy = null)
    {
        UpdateTime = DateTime.Now;
        UpdatedBy = updatedBy;
    }
}
public abstract class ParentEntityWithOutId : ISoftDelete
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public long Id { get; protected set; }
    public long CreatedBy { get; protected set; } = 0;
    public long? UpdatedBy { get; protected set; } = 0;
    public bool IsDeleted { get; protected set; } = false;

    public DateTime CreationTime { get; protected set; } = DateTime.Now;
    public DateTime? UpdateTime { get; protected set; }



    protected void MarkUpdated(long? updatedBy = null)
    {
        UpdateTime = DateTime.Now;
        UpdatedBy = updatedBy;
    }
}
