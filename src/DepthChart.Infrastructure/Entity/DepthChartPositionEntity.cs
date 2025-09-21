namespace DepthChart.Infrastructure.Entities;

public class DepthChartPositionEntity
{
    public Guid Id { get; set; }
    public Guid DepthChartId { get; set; }
    public DepthChartEntity DepthChart { get; set; } = default!;
    public string PositionCode { get; set; } = default!;
    public List<DepthChartEntryEntity> Entries { get; set; } = new();
}
