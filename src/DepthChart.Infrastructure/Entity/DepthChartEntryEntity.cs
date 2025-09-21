namespace DepthChart.Infrastructure.Entities;

public class DepthChartEntryEntity
{
    public Guid Id { get; set; }
    public Guid PositionId { get; set; }
    public Guid PlayerId { get; set; }
    public DepthChartPositionEntity Position { get; set; } = default!;
    public int Depth { get; set; } // 0 = starter
    public PlayerEntity Player { get; set; } = default!;
}
