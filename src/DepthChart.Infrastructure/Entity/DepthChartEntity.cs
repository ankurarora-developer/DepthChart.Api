namespace DepthChart.Infrastructure.Entities;

public class DepthChartEntity
{
    public Guid Id { get; set; }
    public Guid TeamId { get; set; }

    public DateTime UpdatedAtUtc { get; set; }

    public List<DepthChartPositionEntity> Positions { get; set; } = new();
}
