using DepthChart.Application;
using DepthChart.Domain.Entities;
using DepthChart.Domain.Repositories;
using Moq;

public class DepthChartServiceTests
{
    private readonly Mock<IDepthChartRepository> _repoMock;
    private readonly DepthChartService _service;
    private readonly Guid _teamId = Guid.NewGuid();
    private const string Sport = "NFL";
    private const string Position = "QB";

    public DepthChartServiceTests()
    {
        _repoMock = new Mock<IDepthChartRepository>();
        _service = new DepthChartService(_repoMock.Object);
    }

    [Fact]
    public async Task AddPlayerAsync_ThrowsIfTeamDoesNotExist()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);

        var player = new Player("Tom Brady", 12);
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddPlayerAsync(_teamId, Position, player, null));
    }

    [Fact]
    public async Task AddPlayerAsync_ThrowsIfPositionInvalid()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));

        var player = new Player("Tom Brady", 12);
        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.AddPlayerAsync(_teamId, "INVALID", player, null));
    }

    [Fact]
    public async Task AddPlayerAsync_ThrowsIfPlayerAlreadyExists()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddPlayerAsync(_teamId, Position, player, null));
    }

    [Fact]
    public async Task AddPlayerAsync_ThrowsIfDepthTooLarge()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { });

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.AddPlayerAsync(_teamId, Position, player, 1));
    }

    [Fact]
    public async Task AddPlayerAsync_AddsPlayerAtEndIfNoDepth()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);
        var players = new List<Player>();
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);
        _repoMock.Setup(r => r.SavePositionAsync(_teamId, Position, It.IsAny<List<Player>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await _service.AddPlayerAsync(_teamId, Position, player, null);

        _repoMock.Verify(r => r.SavePositionAsync(_teamId, Position, It.Is<List<Player>>(l => l.Contains(player)), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddPlayerAsync_AddsPlayerAtSpecificDepth()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player1 = new Player("Tom Brady", 12);
        var player2 = new Player("Jimmy Garoppolo", 10);
        var players = new List<Player> { player1 };
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);
        _repoMock.Setup(r => r.SavePositionAsync(_teamId, Position, It.IsAny<List<Player>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        await _service.AddPlayerAsync(_teamId, Position, player2, 0);

        _repoMock.Verify(r => r.SavePositionAsync(_teamId, Position, It.Is<List<Player>>(l => l[0].Name == "Jimmy Garoppolo"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemovePlayerAsync_ThrowsIfTeamDoesNotExist()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Team?)null);
        var player = new Player("Tom Brady", 12);

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.RemovePlayerAsync(_teamId, Position, player));
    }

    [Fact]
    public async Task RemovePlayerAsync_ThrowsIfPositionInvalid()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            _service.RemovePlayerAsync(_teamId, "INVALID", player));
    }

    [Fact]
    public async Task RemovePlayerAsync_ReturnsEmptyIfPlayerNotFound()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player>());

        var result = await _service.RemovePlayerAsync(_teamId, Position, player);
        Assert.Empty(result);
    }

    [Fact]
    public async Task RemovePlayerAsync_RemovesPlayer()
    {
        _repoMock.Setup(r => r.GetTeamAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Team(_teamId, "Test", Sport));
        var player = new Player("Tom Brady", 12);
        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Player> { player });
        _repoMock.Setup(r => r.SavePositionAsync(_teamId, Position, It.IsAny<List<Player>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask)
            .Verifiable();

        var result = await _service.RemovePlayerAsync(_teamId, Position, player);
        Assert.Single(result);
        Assert.Equal(player.Name, result[0].Name);
        _repoMock.Verify(r => r.SavePositionAsync(_teamId, Position, It.Is<List<Player>>(l => l.Count == 0), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetBackupsAsync_ReturnsBackups()
    {
        var starter = new Player("Tom Brady", 12);
        var backup1 = new Player("Jimmy Garoppolo", 10);
        var backup2 = new Player("Blaine Gabbert", 11);
        var players = new List<Player> { starter, backup1, backup2 };

        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        var result = await _service.GetBackupsAsync(_teamId, Position, starter);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Jimmy Garoppolo");
        Assert.Contains(result, p => p.Name == "Blaine Gabbert");
    }

    [Fact]
    public async Task GetBackupsAsync_ReturnsEmptyIfNoBackups()
    {
        var starter = new Player("Tom Brady", 12);
        var players = new List<Player> { starter };

        _repoMock.Setup(r => r.GetPositionAsync(_teamId, Position, It.IsAny<CancellationToken>()))
            .ReturnsAsync(players);

        var result = await _service.GetBackupsAsync(_teamId, Position, starter);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetFullDepthChartAsync_DelegatesToRepository()
    {
        var expected = new Dictionary<string, IReadOnlyList<Player>>();
        _repoMock.Setup(r => r.GetFullChartAsync(_teamId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await _service.GetFullDepthChartAsync(_teamId);
        Assert.Same(expected, result);
    }
}