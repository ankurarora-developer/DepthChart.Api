using DepthChart.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.InMemory; 
using DepthChart.Infrastructure;
using DepthChart.Infrastructure.Entities;
using DepthChart.Infrastructure.Repositories;

namespace DepthChart.UnitTests;

public class DepthChartRepositoryTests
{
    private DepthChartDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<DepthChartDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new DepthChartDbContext(options);
    }

    private Guid SeedTeam(DepthChartDbContext db, string sport = "NFL")
    {
        var teamId = Guid.NewGuid();
        db.Teams.Add(new TeamEntity
        {
            Id = teamId,
            Name = "Test Team",
            Sport = sport,
            CreatedAt = DateTime.UtcNow
        });
        db.SaveChanges();
        return teamId;
    }

    [Fact]
    public async Task GetTeamAsync_ReturnsTeam_WhenExists()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);

        var team = await repo.GetTeamAsync(teamId);
        Assert.NotNull(team);
        Assert.Equal(teamId, team!.Id);
    }

    [Fact]
    public async Task GetTeamAsync_ReturnsNull_WhenNotExists()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var team = await repo.GetTeamAsync(Guid.NewGuid());
        Assert.Null(team);
    }

    [Fact]
    public async Task SavePositionAsync_CreatesAndReadsPosition()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);
        var players = new List<Player> { new("Tom Brady", 12), new("Jimmy Garoppolo", 10) };

        await repo.SavePositionAsync(teamId, "QB", players);
        var result = await repo.GetPositionAsync(teamId, "QB");

        Assert.Equal(2, result.Count);
        Assert.Equal("Tom Brady", result[0].Name);
        Assert.Equal("Jimmy Garoppolo", result[1].Name);
    }

    [Fact]
    public async Task SavePositionAsync_UpdatesPositionOrder()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);
        var players = new List<Player> { new("Tom Brady", 12), new("Jimmy Garoppolo", 10) };

        await repo.SavePositionAsync(teamId, "QB", players);
        // Reverse order
        players.Reverse();
        await repo.SavePositionAsync(teamId, "QB", players);
        var result = await repo.GetPositionAsync(teamId, "QB");

        Assert.Equal("Jimmy Garoppolo", result[0].Name);
        Assert.Equal("Tom Brady", result[1].Name);
    }

    [Fact]
    public async Task GetPositionAsync_ReturnsEmpty_WhenNoChart()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);
        var result = await repo.GetPositionAsync(teamId, "QB");
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullChartAsync_ReturnsAllPositions()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);
        await repo.SavePositionAsync(teamId, "QB", new List<Player> { new("Tom Brady", 12) });

        var chart = await repo.GetFullChartAsync(teamId);
        Assert.True(chart.ContainsKey("QB"));
        Assert.Single(chart["QB"]);
    }

    [Fact]
    public async Task SavePositionAsync_RemovesMissingPlayers()
    {
        using var db = CreateDbContext();
        var repo = new DepthChartRepository(db);
        var teamId = SeedTeam(db);
        var players = new List<Player> { new("Tom Brady", 12), new("Jimmy Garoppolo", 10) };
        await repo.SavePositionAsync(teamId, "QB", players);

        // Remove one player
        players.RemoveAt(1);
        await repo.SavePositionAsync(teamId, "QB", players);
        var result = await repo.GetPositionAsync(teamId, "QB");

        Assert.Single(result);
        Assert.Equal("Tom Brady", result[0].Name);
    }
}