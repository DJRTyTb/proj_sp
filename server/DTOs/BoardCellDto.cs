namespace Server.DTOs;

public record BoardCellDto
{
    public required int Row { get; init; }

    public required int Col { get; init; }

    public required string Color { get; init; }
}