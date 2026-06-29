using Microsoft.AspNetCore.SignalR;
using Server.DTOs;
using Server.Enums;
using Server.Hubs;
using Server.Models;

namespace Server.Services.Infrastructure;

public class GameCleanupService(
    IServiceProvider serviceProvider,
    GameLockProvider gameLockProvider,
    IHubContext<GameHub> hubContext,
    ILogger<GameCleanupService> logger) : BackgroundService
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;
    private readonly GameLockProvider _gameLockProvider = gameLockProvider;
    private readonly IHubContext<GameHub> _hubContext = hubContext;
    private readonly ILogger<GameCleanupService> _logger = logger;

    public static readonly int WaitingTimeoutTime = 15;
    public static readonly int FinishedTimeoutTime = 10;
    public static readonly int DisconnectTimeoutTime = 1;

    private static readonly TimeSpan WaitingTimeout = TimeSpan.FromMinutes(WaitingTimeoutTime);
    private static readonly TimeSpan FinishedTimeout = TimeSpan.FromMinutes(FinishedTimeoutTime);
    private static readonly TimeSpan DisconnectTimeout = TimeSpan.FromMinutes(DisconnectTimeoutTime);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await CleanupGames();
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task CleanupGames()
    {
        using IServiceScope scope = _serviceProvider.CreateScope();

        GameManager gameManager = scope.ServiceProvider.GetRequiredService<GameManager>();

        List<Guid> gamesToDelete = [];

        foreach (Game game in gameManager.GetAllGames())
        {
            if (ShouldRemoveWaitingGame(game))
            {
                gamesToDelete.Add(game.Id);
                continue;
            }

            if (ShouldRemoveFinishedGame(game))
            {
                gamesToDelete.Add(game.Id);
                continue;
            }

            if (await ProcessDisconnectedGame(scope, game)) continue;
        }

        foreach (Guid id in gamesToDelete)
        {
            gameManager.RemoveGame(id);
            GameLogs.GameRemoved(_logger, id, null);
            _gameLockProvider.RemoveLock(id);
        }

        await Task.CompletedTask;
    }

    private async Task<bool> ProcessDisconnectedGame(IServiceScope scope, Game game)
    {
        if (game.Status != GameStatus.InProgress) return false;

        Player? disconnected = game.Players.FirstOrDefault(p => !p.IsConnected);

        if (disconnected == null) return false;
        if (disconnected.DisconnectedAt == null) return false;
        if (DateTime.UtcNow - disconnected.DisconnectedAt <= DisconnectTimeout) return false;

        GameService gameService = scope.ServiceProvider.GetRequiredService<GameService>();
        gameService.FinishByDisconnect(game.Id, disconnected);
        GameLogs.DisconnectTimeout(_logger, game.Id, null);

        GameStateDto state = GameStateMapper.ToDto(game);
        await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("BoardUpdated", state);
        await _hubContext.Clients.Group(game.Id.ToString()).SendAsync("GameFinished", state.Winner);

        return true;
    }

    private static bool ShouldRemoveWaitingGame(Game game)
    {
        return game.Status == GameStatus.WaitingForPlayers &&
               DateTime.UtcNow - game.CreatedAt > WaitingTimeout;
    }

    private static bool ShouldRemoveFinishedGame(Game game)
    {
        return game.Status == GameStatus.Finished &&
               game.FinishedAt != null &&
               DateTime.UtcNow - game.FinishedAt > FinishedTimeout;
    }
}