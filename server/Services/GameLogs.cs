using Microsoft.Extensions.Logging;

namespace Server.Services;

public static class GameLogs
{
    public static readonly Action<ILogger, string, Exception?> GameCreated =
        LoggerMessage.Define<string>(
            LogLevel.Information,
            new EventId(1, nameof(GameCreated)),
            "Game created by player {PlayerName}");

    public static readonly Action<ILogger, string, Guid, Exception?> PlayerJoined =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(2, nameof(PlayerJoined)),
            "Player {PlayerName} joined game {GameId}");

    public static readonly Action<ILogger, string, Guid, Exception?> PlayerDisconnected =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Warning,
            new EventId(3, nameof(PlayerDisconnected)),
            "Player {PlayerName} disconnected from game {GameId}");

    public static readonly Action<ILogger, string, Guid, Exception?> PlayerReconnected =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(4, nameof(PlayerReconnected)),
            "Player {PlayerName} reconnected to game {GameId}");

    public static readonly Action<ILogger, string, string, int, int, Guid, Exception?> InitialRemoval =
        LoggerMessage.Define<string, string, int, int, Guid>(
            LogLevel.Information,
            new EventId(5, nameof(InitialRemoval)),
            "Player {PlayerName} removed {Color} frog at ({Row}, {Col}) in game {GameId}");

    public static readonly Action<ILogger, string, string, Guid, Exception?> MoveMade =
        LoggerMessage.Define<string, string, Guid>(
            LogLevel.Information,
            new EventId(6, nameof(MoveMade)),
            "Player {PlayerName} made move {Path} in game {GameId}");

    public static readonly Action<ILogger, string, Guid, Exception?> PlayerPassed =
        LoggerMessage.Define<string, Guid>(
            LogLevel.Information,
            new EventId(7, nameof(PlayerPassed)),
            "Player {PlayerName} passed in game {GameId}");

    public static readonly Action<ILogger, Guid, string, Exception?> GameFinished =
        LoggerMessage.Define<Guid, string>(
            LogLevel.Information,
            new EventId(8, nameof(GameFinished)),
            "Game {GameId} finished. Winner: {Winner}");

    public static readonly Action<ILogger, Guid, Exception?> GameRemoved =
        LoggerMessage.Define<Guid>(
            LogLevel.Information,
            new EventId(9, nameof(GameRemoved)),
            "Game {GameId} removed from memory");

    public static readonly Action<ILogger, Guid, Exception?> DisconnectTimeout =
        LoggerMessage.Define<Guid>(
            LogLevel.Warning,
            new EventId(10, nameof(DisconnectTimeout)),
            "Game {GameId} finished because of disconnect timeout");

    public static readonly Action<ILogger, Guid, int, Exception?> ScheduledRemoval =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Information,
            new EventId(11, nameof(ScheduledRemoval)),
            "Game {GameId} will be removed in {Minutes} minutes");

    public static readonly Action<ILogger, Guid, int, Exception?> CleanupTimeout =
        LoggerMessage.Define<Guid, int>(
            LogLevel.Information,
            new EventId(12, nameof(CleanupTimeout)),
            "Game {GameId} started. Lifetime limit: {Minutes} minutes");
}