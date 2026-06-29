using Server.Enums;
using Server.Models;

namespace Server.Services;

public class GameService(
    GameManager gameManager,
    BoardGenerator boardGenerator,
    MoveValidator moveValidator)
{
    private readonly GameManager _gameManager = gameManager;
    private readonly BoardGenerator _boardGenerator = boardGenerator;
    private readonly MoveValidator _moveValidator = moveValidator;

    public Game CreateGame(string playerName, string connectionId)
    {
        Player player = new(playerName)
        {
            ConnectionId = connectionId,
            Color = FrogColor.Green
        };

        return _gameManager.CreateGame(player);
    }

    public Game JoinGame(Guid gameId, string playerName, string connectionId)
    {
        Game game = _gameManager.GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        Player? disconnectedPlayer = game.Players.FirstOrDefault(p =>
            p.Name == playerName && !p.IsConnected);

        if (disconnectedPlayer != null)
        {
            disconnectedPlayer.ConnectionId = connectionId;
            disconnectedPlayer.IsConnected = true;
            disconnectedPlayer.DisconnectedAt = null;

            return game;
        }

        if (game.Players.Any(p => p.Name == playerName && p.IsConnected))
            throw new InvalidOperationException("Player with this name is already connected.");

        if (game.Players.Count >= 2)
            throw new InvalidOperationException("Game is full.");

        Player player = new(playerName)
        {
            ConnectionId = connectionId,
            Color = FrogColor.Orange
        };

        game.Players.Add(player);

        game.Board = _boardGenerator.Generate();
        game.Status = GameStatus.InProgress;

        return game;
    }

    public void RemoveInitialFrog(Guid gameId, Guid playerToken, Position position)
    {
        Game game = GetRunningGame(gameId);

        Player player = GetPlayer(game, playerToken);

        if (player != game.CurrentPlayer)
            throw new InvalidOperationException("Not your turn.");

        if (player.HasUsedInitialRemoval)
            throw new InvalidOperationException("Initial removal already used.");

        if (!game.Board.IsOccupied(position))
            throw new InvalidOperationException("No frog at position.");

        game.Board.RemoveFrog(position);

        player.HasUsedInitialRemoval = true;

        game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
    }

    public void MakeMove(Guid gameId, Guid playerToken, List<Position> path)
    {
        Game game = GetRunningGame(gameId);

        Player player = GetPlayer(game, playerToken);

        if (player != game.CurrentPlayer)
            throw new InvalidOperationException("Not your turn.");

        if (!player.HasUsedInitialRemoval)
            throw new InvalidOperationException("Initial removal required.");

        if (!_moveValidator.IsMoveValid(game.Board, path, player.Color))
            throw new InvalidOperationException("Invalid move.");

        for (int i = 0; i < path.Count - 1; i++)
        {
            Position from = path[i];
            Position to = path[i + 1];

            Position jumped = _moveValidator.GetJumpedPosition(from, to);

            game.Board.RemoveFrog(jumped);
            game.Board.MoveFrog(from, to);
        }

        player.HasMadeJump = true;
        game.LastJumpPlayer = player;
        game.ConsecutivePasses = 0;

        Position finalPosition = path[^1];

        if (game.Board.IsSwamp(finalPosition))
            game.Board.RemoveFrog(finalPosition);

        EndTurn(game);
    }

    public void PassTurn(Guid gameId, Guid playerToken)
    {
        Game game = GetRunningGame(gameId);
        Player player = GetPlayer(game, playerToken);

        if (player != game.CurrentPlayer)
            throw new InvalidOperationException("Not your turn.");
        if (!player.HasUsedInitialRemoval) return;

        game.LastGameMessage = $"⏭ {player.Name} passed.";
        game.ConsecutivePasses++;

        if (game.ConsecutivePasses >= 2)
        {
            game.Status = GameStatus.Finished;
            game.FinishedAt = DateTime.UtcNow;
            return;
        }

        EndTurn(game);
    }

    public void CheckAutomaticPass(Game game)
    {
        while (true)
        {
            Player player = game.CurrentPlayer;

            if (!player.HasUsedInitialRemoval) return;
            if (_moveValidator.HasAnyValidMove(game.Board, player.Color)) return;

            game.LastGameMessage = $"⏭ {player.Name} passed.";
            game.ConsecutivePasses++;

            if (game.ConsecutivePasses >= 2)
            {
                game.Status = GameStatus.Finished;
                game.FinishedAt = DateTime.UtcNow;
                return;
            }

            game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;
        }
    }

    private void EndTurn(Game game)
    {
        game.CurrentPlayerIndex = 1 - game.CurrentPlayerIndex;

        CheckAutomaticPass(game);
    }

    private Game GetRunningGame(Guid gameId)
    {
        Game game = _gameManager.GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        if (game.Status != GameStatus.InProgress)
            throw new InvalidOperationException("Game not running.");

        return game;
    }

    public void FinishByDisconnect(Guid gameId, Player disconnectedPlayer)
    {
        Game game = _gameManager.GetGame(gameId)
            ?? throw new InvalidOperationException("Game not found.");

        if (game.Status != GameStatus.InProgress)
        {
            return;
        }

        Player winner = game.Players.First(p => p != disconnectedPlayer);

        game.Status = GameStatus.Finished;
        game.FinishedAt = DateTime.UtcNow;

        game.FinishedMessage = $"{winner.Name} wins by disconnect.";
    }

    private static Player GetPlayer(Game game, Guid token)
    {
        return game.Players.FirstOrDefault(p => p.Token == token)
            ?? throw new InvalidOperationException("Player not found.");
    }
}