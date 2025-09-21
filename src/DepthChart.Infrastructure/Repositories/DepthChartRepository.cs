using DepthChart.Domain.Entities;
using DepthChart.Domain.Repositories;
using DepthChart.Infrastructure.Entities;
using Microsoft.EntityFrameworkCore;

namespace DepthChart.Infrastructure.Repositories;

public sealed class DepthChartRepository : IDepthChartRepository
{
    private readonly DepthChartDbContext _db;

    public DepthChartRepository(DepthChartDbContext db) => _db = db;

    public async Task<Team?> GetTeamAsync(Guid teamId, CancellationToken ct = default)
    {
        var t = await _db.Teams.AsNoTracking().SingleOrDefaultAsync(x => x.Id == teamId, ct);
        return t is null ? null : new Team(t.Id, t.Name, t.Sport);
    }

    // --------- READ: one position ----------
    public async Task<List<Player>> GetPositionAsync(Guid teamId, string position, CancellationToken ct = default)
    {
        // Load chart + ONLY the requested position + its entries
        var dc = await _db.DepthCharts
            .Include(dc => dc.Positions.Where(p => p.PositionCode == position))
                .ThenInclude(p => p.Entries)
            .AsNoTracking()
            .SingleOrDefaultAsync(dc => dc.TeamId == teamId, ct);

        if (dc is null) return [];

        var pos = dc.Positions.SingleOrDefault();
        if (pos is null || pos.Entries.Count == 0) return new List<Player>();

        var ids = pos.Entries.Select(e => e.PlayerId).Distinct().ToList();
        var players = await _db.Players
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        return pos.Entries
            .OrderBy(e => e.Depth)
            .Select(e =>
            {
                if (!players.TryGetValue(e.PlayerId, out var pe)) return null;
                return new Player(pe.Name, pe.Number);
            })
            .Where(p => p is not null)!
            .ToList()!;
    }

    // --------- WRITE: one position ----------
    public async Task SavePositionAsync(Guid teamId, string position, List<Player> orderedPlayers, CancellationToken ct = default)
    {
        // 1. Load team’s depth chart and requested position
        var dc = await _db.DepthCharts
            .Include(x => x.Positions.Where(p => p.PositionCode == position))
                .ThenInclude(p => p.Entries)
            .SingleOrDefaultAsync(x => x.TeamId == teamId, ct);

        if (dc is null)
        {
            dc = new DepthChartEntity
            {
                Id = Guid.NewGuid(),
                TeamId = teamId,
                UpdatedAtUtc = DateTime.UtcNow,
                Positions = []
            };
            _db.DepthCharts.Add(dc);
        }
        else
        {
            dc.UpdatedAtUtc = DateTime.UtcNow;
        }

        // 2. Ensure position exists
        var pos = dc.Positions.SingleOrDefault();
        if (pos is null)
        {
            pos = new DepthChartPositionEntity
            {
                Id = Guid.NewGuid(),
                DepthChartId = dc.Id,
                PositionCode = position,
                Entries = new List<DepthChartEntryEntity>()
            };
            _db.Positions.Add(pos);
            //dc.Positions.Add(pos);
        }

        // 3. Ensure all players exist in DB
        var ensuredPlayers = new List<PlayerEntity>();

        foreach (var pl in orderedPlayers)
        {
            var match = await _db.Players
                .FirstOrDefaultAsync(x => x.Name.ToLower() == pl.Name.ToLower() && x.Number == pl.Number, ct);

            if (match is null)
            {
                match = new PlayerEntity
                {
                    Id = Guid.NewGuid(),
                    Name = pl.Name,
                    Number = pl.Number,
                    CreatedAt = DateTime.UtcNow
                };
                _db.Players.Add(match);
            }

            ensuredPlayers.Add(match);
        }

        // 4. Sync entries for this position

        // update or insert
        pos.Entries.Clear();
        for (int i = 0; i < ensuredPlayers.Count; i++)
        {
            var pl = ensuredPlayers[i];
            var newEntry = new DepthChartEntryEntity
            {
                Id = Guid.NewGuid(),
                PositionId = pos.Id,
                PlayerId = pl.Id,
                Depth = i
            };
            pos.Entries.Add(newEntry);
            _db.Entries.Add(newEntry);
        }

        await _db.SaveChangesAsync(ct);
    }


    // --------- READ: full chart ----------
    public async Task<IReadOnlyDictionary<string, IReadOnlyList<Player>>> GetFullChartAsync(Guid teamId, CancellationToken ct = default)
    {
        var dc = await _db.DepthCharts
            .Include(x => x.Positions)
                .ThenInclude(p => p.Entries)
            .AsNoTracking()
            .SingleOrDefaultAsync(x => x.TeamId == teamId, ct);

        var result = new Dictionary<string, IReadOnlyList<Player>>(StringComparer.OrdinalIgnoreCase);

        if (dc is null) return result;

        // Gather all player IDs once
        var ids = dc.Positions.SelectMany(p => p.Entries.Select(e => e.PlayerId)).Distinct().ToList();
        var players = await _db.Players
            .Where(p => ids.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, ct);

        foreach (var pos in dc.Positions.OrderBy(p => p.PositionCode))
        {
            var list = pos.Entries
                .OrderBy(e => e.Depth)
                .Select(e => players.TryGetValue(e.PlayerId, out var pe)
                    ? new Player(pe.Name, pe.Number)
                    : null)
                .Where(p => p is not null)!
                .ToList()!;

            result[pos.PositionCode] = list;
        }

        return result;
    }
}

