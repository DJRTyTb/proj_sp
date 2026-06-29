using Microsoft.AspNetCore.SignalR;
using Server.DTOs;
using Server.Models;
using Server.Services;
using Server.Services.Infrastructure;

namespace Server.Hubs;

public class GameHub(
    GameService gameService,
    GameManager gameManager,
    GameLockProvider gameLockProvider,
    ILogger<GameHub> logger) : Hub
{
    private readonly GameService _gameService = gameService;
    private readonly GameManager _gameManager = gameManager;
    private readonly GameLockProvider _gameLockProvider = gameLockProvider;
    private readonly ILogger<GameHub> _logger = logger;

    public async Task CreateGame(string playerName)
    {
        try
        {
            Game game = _gameService.CreateGame(playerName, Context.ConnectionId);
            GameLogs.GameCreated(_logger, playerName, null);

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id.ToString());

            await Clients.Caller.SendAsync(
                "GameCreated",
                game.Id,
                game.Players[0].Token,
                game.Players[0].Color.ToString().ToLower());
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task JoinGame(string gameId, string playerName)
    {
        try
        {
            Game game = _gameManager.GetGame(Guid.Parse(gameId))
                ?? throw new HubException("Game not found.");

            bool isReconnect = game.Players.Any(p => p.Name == playerName && !p.IsConnected);

            game = _gameService.JoinGame(game.Id, playerName, Context.ConnectionId);

            await Groups.AddToGroupAsync(Context.ConnectionId, game.Id.ToString());

            Player player = game.Players.First(p => p.ConnectionId == Context.ConnectionId);
            if (!isReconnect) GameLogs.PlayerJoined(_logger, player.Name, game.Id, null);

            await Clients.Caller.SendAsync(
                "JoinedGame",
                game.Id,
                player.Token,
                player.Color.ToString().ToLower());

            GameStateDto state = GameStateMapper.ToDto(game);

            if (!game.IsStartedBroadcasted)
            {
                game.IsStartedBroadcasted = true;
                await Clients.Group(game.Id.ToString()).SendAsync("GameStarted", state);
            }
            else
            {
                await Clients.Caller.SendAsync("BoardUpdated", state);

                if (isReconnect)
                {
                    GameLogs.PlayerReconnected(_logger, player.Name, game.Id, null);

                    await Clients.OthersInGroup(game.Id.ToString())
                        .SendAsync("PlayerReconnected", player.Name);
                }
            }
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
    }

    public async Task RemoveInitialFrog(string gameId, Guid token, int row, int col)
    {
        Guid id = Guid.Parse(gameId);

        SemaphoreSlim gameLock = _gameLockProvider.GetLock(id);

        await gameLock.WaitAsync();

        try
        {
            Game game = _gameManager.GetGame(id) ?? throw new HubException("Game not found.");
            Player player = game.Players.First(p => p.Token == token);
            Frog frog = game.Board.GetFrog(new(row + 1, col + 1)) ?? throw new HubException("No frog at position.");
            string color = frog.Color.ToString().ToLower();

            _gameService.RemoveInitialFrog(game.Id, token, new(row + 1, col + 1));
            GameLogs.InitialRemoval(_logger, player.Name, color, row, col, game.Id, null);

            GameStateDto state = GameStateMapper.ToDto(game);

            await Clients.Group(game.Id.ToString()).SendAsync("BoardUpdated", state);
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
        finally
        {
            gameLock.Release();
        }
    }

    public async Task MakeMove(string gameId, Guid token, List<Position> path)
    {
        Guid id = Guid.Parse(gameId);

        SemaphoreSlim gameLock = _gameLockProvider.GetLock(id);

        await gameLock.WaitAsync();

        try
        {
            Game game = _gameManager.GetGame(id) ?? throw new HubException("Game not found.");
            Player player = game.Players.First(p => p.Token == token);

            List<Position> serverPath = [.. path.Select(p => new Position(p.Row + 1, p.Col + 1))];
            string pathString = string.Join(" -> ", serverPath.Select(p => $"({p.Row}, {p.Col})"));

            _gameService.MakeMove(game.Id, token, serverPath);
            GameLogs.MoveMade(_logger, player.Name, pathString, game.Id, null);

            GameStateDto state = GameStateMapper.ToDto(game);

            await Clients.Group(game.Id.ToString()).SendAsync("BoardUpdated", state);

            if (game.Status == Enums.GameStatus.Finished)
            {
                GameLogs.GameFinished(_logger, game.Id, state.Winner ?? "draw", null);
                GameLogs.ScheduledRemoval(_logger, game.Id, GameCleanupService.FinishedTimeoutTime, null);

                await Clients.Group(game.Id.ToString()).SendAsync("GameFinished", state.Winner);
            }
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
        finally
        {
            gameLock.Release();
        }
    }

    public async Task PassTurn(string gameId, Guid token)
    {
        Guid id = Guid.Parse(gameId);

        SemaphoreSlim gameLock = _gameLockProvider.GetLock(id);

        await gameLock.WaitAsync();

        try
        {
            Game game = _gameManager.GetGame(id) ?? throw new HubException("Game not found.");
            Player player = game.Players.First(p => p.Token == token);

            _gameService.PassTurn(game.Id, token);
            GameLogs.PlayerPassed(_logger, player.Name, game.Id, null);

            GameStateDto state = GameStateMapper.ToDto(game);

            await Clients.Group(game.Id.ToString()).SendAsync("BoardUpdated", state);

            if (game.Status == Enums.GameStatus.Finished)
            {
                GameLogs.GameFinished(_logger, game.Id, state.Winner ?? "draw", null);
                GameLogs.ScheduledRemoval(_logger, game.Id, GameCleanupService.FinishedTimeoutTime, null);

                await Clients.Group(game.Id.ToString()).SendAsync("GameFinished", state.Winner);
            }
        }
        catch (HubException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HubException(ex.Message);
        }
        finally
        {
            gameLock.Release();
        }
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
    {
        foreach (Game game in _gameManager.GetAllGames())
        {
            Player? player = game.Players.FirstOrDefault(p => p.ConnectionId == Context.ConnectionId);

            if (player == null)
                continue;

            player.IsConnected = false;
            player.DisconnectedAt = DateTime.UtcNow;
            GameLogs.PlayerDisconnected(_logger, player.Name, game.Id, null);

            await Clients.Group(game.Id.ToString())
                .SendAsync("PlayerDisconnected", player.Name);
        }

        await base.OnDisconnectedAsync(exception);
    }
}