namespace DepthChart.Infrastructure.Entities;

public class TeamEntity
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string Sport { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}
