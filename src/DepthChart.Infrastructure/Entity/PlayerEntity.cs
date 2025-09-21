namespace DepthChart.Infrastructure.Entities;

public class PlayerEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public int Number { get; set; }
    public DateTime CreatedAt { get; set; }
}
