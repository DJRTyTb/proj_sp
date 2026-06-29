namespace Server.DTOs;

public record PlayerDto
{
    public required string Name { get; init; }

    public required string Color { get; init; }

    public required bool IsCurrentTurn { get; init; }

    public required bool HasUsedInitialRemoval { get; init; }
}