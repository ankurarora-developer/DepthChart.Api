using DepthChart.Domain.Entities;

namespace DepthChart.Domain.Repositories;

public interface IDepthChartRepository
{
    Task<Team?> GetTeamAsync(Guid teamId, CancellationToken ct = default);
    Task<List<Player>> GetPositionAsync(Guid teamId, string position, CancellationToken ct = default);
    Task SavePositionAsync(Guid teamId, string position, List<Player> orderedPlayers, CancellationToken ct = default);
    Task<IReadOnlyDictionary<string, IReadOnlyList<Player>>> GetFullChartAsync(Guid teamId, CancellationToken ct = default);
}
