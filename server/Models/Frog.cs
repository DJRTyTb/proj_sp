using Server.Enums;

namespace Server.Models;

public record Frog
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public FrogColor Color { get; init; }
}