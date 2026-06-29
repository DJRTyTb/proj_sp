using Server.Models;

namespace Server.Services;

public class GameManager
{
    private readonly Dictionary<Guid, Game> _games = [];
    private readonly Lock _lock = new();

    public Game CreateGame(Player player)
    {
        lock (_lock)
        {
            Game game = new()
            {
                Players = [player]
            };

            _games[game.Id] = game;

            return game;
        }
    }

    public Game? GetGame(Guid id)
    {
        lock (_lock)
        {
            return _games.GetValueOrDefault(id);
        }
    }

    public IReadOnlyCollection<Game> GetAllGames()
    {
        lock (_lock)
        {
            return [.. _games.Values];
        }
    }

    public void RemoveGame(Guid id)
    {
        lock (_lock)
        {
            _games.Remove(id);
        }
    }
}