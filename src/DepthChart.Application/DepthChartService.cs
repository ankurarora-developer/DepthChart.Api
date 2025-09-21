using DepthChart.Application.Validation;
using DepthChart.Domain.Entities;
using DepthChart.Domain.Repositories;

namespace DepthChart.Application;

public class DepthChartService: IDepthChartService
{
    private readonly IDepthChartRepository _repo;

    public DepthChartService(IDepthChartRepository repo)
    {
        _repo = repo;
    }

    public async Task AddPlayerAsync(Guid teamId, string position, Player player, int? depth, CancellationToken ct = default)
    {
        var team = await _repo.GetTeamAsync(teamId, ct);
        if (team is null)
            throw new InvalidOperationException($"Team {teamId} does not exist.");

        if (!SportRules.IsValidPosition(team.Sport, position))
            throw new ArgumentException($"Position {position} is not valid for {team.Sport}.");

        var players = await _repo.GetPositionAsync(teamId, position, ct) ?? new List<Player>();

        if (players.Any(p => p.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase) && p.Number == player.Number))
            throw new InvalidOperationException($"{player.Name} #{player.Number} is already in the depth chart at {position}.");

        if (depth.HasValue && depth.Value > players.Count)
            throw new InvalidOperationException(
                $"Cannot add {player.Name} #{player.Number} at depth {depth.Value} " +
                $"because depth {players.Count} must be filled first.");

        if (!depth.HasValue || depth.Value >= players.Count)
            players.Add(player);
        else
            players.Insert(depth.Value, player);

        await _repo.SavePositionAsync(teamId, position, players, ct);
    }

    public async Task<List<Player>> RemovePlayerAsync(Guid teamId, string position, Player player, CancellationToken ct = default)
    {
        var team = await _repo.GetTeamAsync(teamId, ct);
        if (team is null)
            throw new InvalidOperationException($"Team {teamId} does not exist.");

        if (!SportRules.IsValidPosition(team.Sport, position))
            throw new ArgumentException($"Position {position} is not valid for {team.Sport}.");

        var players = await _repo.GetPositionAsync(teamId, position, ct) ?? new List<Player>();

        var removed = players.RemoveAll(p => p.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase) && p.Number == player.Number);
        if (removed == 0)
            return []; // empty list if not found

        await _repo.SavePositionAsync(teamId, position, players, ct);
        return [player];
    }

    public async Task<List<Player>> GetBackupsAsync(
        Guid teamId, string position, Player player, CancellationToken ct = default)
    {
        var players = await _repo.GetPositionAsync(teamId, position, ct);

        var idx = players.FindIndex(p =>
            p.Name.Equals(player.Name, StringComparison.OrdinalIgnoreCase) &&
            p.Number == player.Number);

        if (idx < 0 || idx >= players.Count - 1) return [];
        return [.. players.Skip(idx + 1)];
    }

    public Task<IReadOnlyDictionary<string, IReadOnlyList<Player>>> GetFullDepthChartAsync(
         Guid teamId, CancellationToken ct = default)
         => _repo.GetFullChartAsync(teamId, ct);
}
