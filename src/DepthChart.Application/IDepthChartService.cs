using DepthChart.Domain.Entities;

namespace DepthChart.Application;

public interface IDepthChartService
{
    Task AddPlayerAsync(Guid teamId, string position, Player player, int? depth, CancellationToken ct = default);
    Task<List<Player>> RemovePlayerAsync(Guid teamId, string position, Player player, CancellationToken ct = default);
    Task<List<Player>> GetBackupsAsync(Guid teamId, string position, Player player, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, IReadOnlyList<Player>>> GetFullDepthChartAsync(Guid teamId, CancellationToken ct = default);
}