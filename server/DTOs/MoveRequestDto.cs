using Server.Models;

namespace Server.DTOs;

public record MoveRequestDto
{
    public required List<Position> Path { get; init; }
}