namespace DepthChart.Contracts;

public record AddPlayerRequest(string Position, string Name, int Number, int? PositionDepth);
public record RemovePlayerRequest(string Position, string Name, int Number);
