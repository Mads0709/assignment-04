namespace Assignment.Infrastructure;


public class Tag
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public virtual ICollection<WorkItem> WorkItems { get; set; } = null!; //many-to-many like a join statemen
}
