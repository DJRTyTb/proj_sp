using Server.Enums;

namespace Server.Models;

public class Game
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public Board Board { get; set; } = new();

    public List<Player> Players { get; init; } = [];

    public int CurrentPlayerIndex { get; set; }

    public int ConsecutivePasses { get; set; }

    public bool IsStartedBroadcasted { get; set; }

    public Player? LastJumpPlayer { get; set; }

    public string? LastGameMessage { get; set; }

    public string? FinishedMessage { get; set; }

    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;

    public DateTime? FinishedAt { get; set; }

    public GameStatus Status { get; set; } = GameStatus.WaitingForPlayers;

    public Player CurrentPlayer => Players[CurrentPlayerIndex];

    public Player OpponentPlayer => Players[1 - CurrentPlayerIndex];
}