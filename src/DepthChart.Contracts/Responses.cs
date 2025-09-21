using DepthChart.Domain.Entities;

namespace DepthChart.Contracts;

public record DepthChartResponse(Dictionary<string, IReadOnlyList<Player>> Positions);
