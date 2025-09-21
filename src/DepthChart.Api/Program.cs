using DepthChart.Api.Extensions;
using DepthChart.Api.Middleware;
using DepthChart.Application;
using DepthChart.Contracts;
using DepthChart.Domain.Entities;

var builder = WebApplication.CreateBuilder(args);

// --- Services ---
builder.RegisterBuilderServices();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Middleware ---
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.MapGet("/health", () => Results.Ok("DepthChart.Api running"));

// --- Routes ---

// GET full depth chart
app.MapGet("/teams/{teamId:guid}/depthchart",
    async (Guid teamId, IDepthChartService svc, CancellationToken ct) =>
    {
        if (teamId == Guid.Empty)
            return Results.BadRequest("Team ID is required.");

        var chart = await svc.GetFullDepthChartAsync(teamId, ct);
        return Results.Ok(chart);
    });

// ADD player
app.MapPost("/teams/{teamId:guid}/depthchart/add",
    async (Guid teamId, AddPlayerRequest req, IDepthChartService svc, CancellationToken ct) =>
    {
        if (teamId == Guid.Empty)
            return Results.BadRequest("Team ID is required.");
        if (string.IsNullOrWhiteSpace(req.Position))
            return Results.BadRequest("Position is required.");
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest("Player name is required.");
        if (req.Number <= 0)
            return Results.BadRequest("Player number must be positive.");
        if (req.PositionDepth is < 0)
            return Results.BadRequest("Depth must be >= 0.");

        var player = new Player(req.Name, req.Number);
        await svc.AddPlayerAsync(teamId, req.Position, player, req.PositionDepth, ct);
        return Results.NoContent();
    });

// REMOVE player
app.MapPost("/teams/{teamId:guid}/depthchart/remove",
    async (Guid teamId, RemovePlayerRequest req, IDepthChartService svc, CancellationToken ct) =>
    {
        if (teamId == Guid.Empty)
            return Results.BadRequest("Team ID is required.");
        if (string.IsNullOrWhiteSpace(req.Position))
            return Results.BadRequest("Position is required.");
        if (string.IsNullOrWhiteSpace(req.Name))
            return Results.BadRequest("Player name is required.");
        if (req.Number <= 0)
            return Results.BadRequest("Player number must be positive.");

        var player = new Player(req.Name, req.Number);
        var response = await svc.RemovePlayerAsync(teamId, req.Position, player, ct);
        return Results.Ok(response);
    });

// GET backups
app.MapGet("/teams/{teamId:guid}/depthchart/{position}/{name}/{number}/backups",
    async (Guid teamId, string position, string name, int number, IDepthChartService svc, CancellationToken ct) =>
    {
        if (teamId == Guid.Empty)
            return Results.BadRequest("Team ID is required.");
        if (string.IsNullOrWhiteSpace(position))
            return Results.BadRequest("Position is required.");
        if (string.IsNullOrWhiteSpace(name))
            return Results.BadRequest("Player name is required.");
        if (number <= 0)
            return Results.BadRequest("Player number must be positive.");

        var player = new Player(name, number);
        var backups = await svc.GetBackupsAsync(teamId, position, player, ct);
        return Results.Ok(backups);
    });

app.Run();

public partial class Program { }
