namespace Server.DTOs;

public record GameStateDto
{
    public required Guid GameId { get; init; }

    public required string CurrentPlayer { get; init; }

    public required IEnumerable<PlayerDto> Players { get; init; }

    public required IEnumerable<BoardCellDto> Cells { get; init; }

    public required bool IsFinished { get; init; }

    public string? Winner { get; init; }

    public string? Message { get; init; }
}