namespace Domain;

public abstract class ParentLookupInt : ParentLookup
{

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public int Id { get; protected set; }
}
public abstract class ParentLookupEnum<T> : ParentLookup where T : Enum
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.None)]
    public T Id { get; protected set; }
}
public abstract class ParentLookup : ISoftDelete
{
    public string NameAr { get; protected set; } = string.Empty;
    public string NameEn { get; protected set; } = string.Empty;
    public bool IsDeleted { get; protected set; }
    public long CreatedBy { get; protected set; } = 0;
    public long? UpdatedBy { get; protected set; }
    public DateTime CreationTime { get; protected set; } = DateTime.Now;
    public DateTime? UpdateTime { get; protected set; }
}
