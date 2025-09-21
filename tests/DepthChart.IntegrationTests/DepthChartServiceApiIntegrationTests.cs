using System.Net;
using System.Net.Http.Json;
using DepthChart.Contracts;
using DepthChart.Domain.Entities;
using DepthChart.Infrastructure;
using DepthChart.Infrastructure.Entities;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DepthChart.IntegrationTests;

public class DepthChartServiceApiIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{

    private readonly WebApplicationFactory<Program> _factory;

    public DepthChartServiceApiIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                // Remove all registrations for DepthChartDbContext and its options
                var descriptors = services
                    .Where(d =>
                        d.ServiceType == typeof(DepthChartDbContext) ||
                        d.ServiceType == typeof(DbContextOptions<DepthChartDbContext>) ||
                        d.ServiceType.FullName?.Contains("DepthChartDbContext") == true)
                    .ToList();

                foreach (var descriptor in descriptors)
                    services.Remove(descriptor);

                // Add in-memory DB
                services.AddDbContext<DepthChartDbContext>(options =>
                {
                    options.UseInMemoryDatabase("DepthChartApiTestDb");
                });

                // Ensure DB is created
                var sp = services.BuildServiceProvider();
                using var scope = sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<DepthChartDbContext>();
                db.Database.EnsureCreated();
            });
        });
    }

    private async Task<Guid> SeedTeamAsync()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<DepthChartDbContext>();
        var teamId = Guid.NewGuid();
        db.Teams.Add(new TeamEntity
        {
            Id = teamId,
            Name = "Test Team",
            Sport = "NFL",
            CreatedAt = DateTime.UtcNow
        });
        await db.SaveChangesAsync();
        return teamId;
    }

    [Fact]
    public async Task AddPlayer_Then_GetFullDepthChart_ReturnsPlayer()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();

        var addReq = new AddPlayerRequest("QB", "Tom Brady", 12, null);
        var addResp = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.NoContent, addResp.StatusCode);

        var chartResp = await client.GetAsync($"/teams/{teamId}/depthchart");
        Assert.Equal(HttpStatusCode.OK, chartResp.StatusCode);

        var chart = await chartResp.Content.ReadFromJsonAsync<Dictionary<string, List<Player>>>();
        Assert.NotNull(chart);
        Assert.True(chart.ContainsKey("QB"));
        Assert.Contains(chart["QB"], p => p.Name == "Tom Brady" && p.Number == 12);
    }

    [Fact]
    public async Task RemovePlayer_RemovesPlayerFromDepthChart()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();

        await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", new AddPlayerRequest("QB", "Tom Brady", 12, null));
        var removeReq = new RemovePlayerRequest("QB", "Tom Brady", 12);
        var removeResp = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/remove", removeReq);
        Assert.Equal(HttpStatusCode.OK, removeResp.StatusCode);

        var chartResp = await client.GetAsync($"/teams/{teamId}/depthchart");
        var chart = await chartResp.Content.ReadFromJsonAsync<Dictionary<string, List<Player>>>();
        Assert.NotNull(chart);
        Assert.True(chart.ContainsKey("QB"));
        Assert.DoesNotContain(chart["QB"], p => p.Name == "Tom Brady");
    }

    [Fact]
    public async Task GetBackups_ReturnsCorrectBackups()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();

        await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", new AddPlayerRequest("QB", "Tom Brady", 12, null));
        await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", new AddPlayerRequest("QB", "Jimmy Garoppolo", 10, null));
        await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", new AddPlayerRequest("QB", "Blaine Gabbert", 11, null));

        var resp = await client.GetAsync($"/teams/{teamId}/depthchart/QB/Tom Brady/12/backups");
        Assert.Equal(HttpStatusCode.OK, resp.StatusCode);

        var backups = await resp.Content.ReadFromJsonAsync<List<Player>>();
        Assert.NotNull(backups);
        Assert.Equal(2, backups.Count);
        Assert.Contains(backups, p => p.Name == "Jimmy Garoppolo");
        Assert.Contains(backups, p => p.Name == "Blaine Gabbert");
    }

    [Fact]
    public async Task AddPlayer_InvalidPosition_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();

        var addReq = new AddPlayerRequest("INVALID", "Tom Brady", 12, null);
        var addResp = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, addResp.StatusCode);
    }

    [Fact]
    public async Task GetFullDepthChart_EmptyTeamId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/teams/{Guid.Empty}/depthchart");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Team ID is required", error);
    }

    [Fact]
    public async Task AddPlayer_EmptyTeamId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var addReq = new AddPlayerRequest("QB", "Tom Brady", 12, null);
        var response = await client.PostAsJsonAsync($"/teams/{Guid.Empty}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Team ID is required", error);
    }

    [Fact]
    public async Task AddPlayer_EmptyPosition_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var addReq = new AddPlayerRequest("", "Tom Brady", 12, null);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Position is required", error);
    }

    [Fact]
    public async Task AddPlayer_EmptyName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var addReq = new AddPlayerRequest("QB", "", 12, null);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Player name is required", error);
    }

    [Fact]
    public async Task AddPlayer_NonPositiveNumber_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var addReq = new AddPlayerRequest("QB", "Tom Brady", 0, null);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Player number must be positive", error);
    }

    [Fact]
    public async Task AddPlayer_NegativeDepth_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var addReq = new AddPlayerRequest("QB", "Tom Brady", 12, -1);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/add", addReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Depth must be >= 0", error);
    }

    [Fact]
    public async Task RemovePlayer_EmptyTeamId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var removeReq = new RemovePlayerRequest("QB", "Tom Brady", 12);
        var response = await client.PostAsJsonAsync($"/teams/{Guid.Empty}/depthchart/remove", removeReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Team ID is required", error);
    }

    [Fact]
    public async Task RemovePlayer_EmptyPosition_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var removeReq = new RemovePlayerRequest("", "Tom Brady", 12);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/remove", removeReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Position is required", error);
    }

    [Fact]
    public async Task RemovePlayer_EmptyName_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var removeReq = new RemovePlayerRequest("QB", "", 12);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/remove", removeReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Player name is required", error);
    }

    [Fact]
    public async Task RemovePlayer_NonPositiveNumber_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var removeReq = new RemovePlayerRequest("QB", "Tom Brady", 0);
        var response = await client.PostAsJsonAsync($"/teams/{teamId}/depthchart/remove", removeReq);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Player number must be positive", error);
    }

    [Fact]
    public async Task GetBackups_EmptyTeamId_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var response = await client.GetAsync($"/teams/{Guid.Empty}/depthchart/QB/Tom Brady/12/backups");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Team ID is required", error);
    }

    [Fact]
    public async Task GetBackups_EmptyPosition_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var response = await client.GetAsync($"/teams/{teamId}/depthchart//Tom Brady/12/backups");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBackups_EmptyName_ReturnsNotFound()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var response = await client.GetAsync($"/teams/{teamId}/depthchart/QB//12/backups");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetBackups_NonPositiveNumber_ReturnsBadRequest()
    {
        var client = _factory.CreateClient();
        var teamId = await SeedTeamAsync();
        var response = await client.GetAsync($"/teams/{teamId}/depthchart/QB/Tom Brady/0/backups");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var error = await response.Content.ReadAsStringAsync();
        Assert.Contains("Player number must be positive", error);
    }
}