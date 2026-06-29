using Server.Enums;
using Server.Models;
using Server.Services;
using Tests.Fakes;

namespace Tests.GameServiceTests;

public static class GameServiceHelpers
{
    public static GameService CreateService(GameManager? manager = null)
    {
        manager ??= new GameManager();

        BoardGenerator generator = new(new FakeRandomProvider());
        MoveValidator validator = new();

        return new(manager, generator, validator);
    }

    public static Game CreateStartedGame(GameService service)
    {
        Game game = service.CreateGame("Alice", "conn1");

        service.JoinGame(game.Id, "Bob", "conn2");

        return game;
    }

    public static Player GetPlayer(Game game, string name)
    {
        return game.Players.First(p => p.Name == name);
    }

    public static void CompleteInitialRemovals(GameService service, Game game)
    {
        Player alice = GetPlayer(game, "Alice");
        Player bob = GetPlayer(game, "Bob");

        service.RemoveInitialFrog(game.Id, alice.Token, new(1, 1));
        service.RemoveInitialFrog(game.Id, bob.Token, new(1, 2));
    }
}