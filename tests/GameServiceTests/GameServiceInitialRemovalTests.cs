using Server.Models;
using Server.Services;
using Server.Enums;

namespace Tests.GameServiceTests;

public class GameServiceInitialRemovalTests
{
    [Fact]
    public void RemoveInitialFrog_RemovesFrog()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = GameServiceHelpers.CreateStartedGame(service);

        var player = GameServiceHelpers.GetPlayer(game, "Alice");

        service.RemoveInitialFrog(game.Id, player.Token, new(1, 1));

        Assert.False(game.Board.IsOccupied(new(1, 1)));
    }

    [Fact]
    public void RemoveInitialFrog_MarksRemovalAsUsed()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = GameServiceHelpers.CreateStartedGame(service);

        var player = GameServiceHelpers.GetPlayer(game, "Alice");

        service.RemoveInitialFrog(game.Id, player.Token, new(1, 1));

        Assert.True(player.HasUsedInitialRemoval);
    }

    [Fact]
    public void RemoveInitialFrog_SwitchesTurn()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = GameServiceHelpers.CreateStartedGame(service);

        var player = GameServiceHelpers.GetPlayer(game, "Alice");

        service.RemoveInitialFrog(game.Id, player.Token, new(1, 1));

        Assert.Equal("Bob", game.CurrentPlayer.Name);
    }

    [Fact]
    public void RemoveInitialFrog_Twice_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = GameServiceHelpers.CreateStartedGame(service);

        var alice = GameServiceHelpers.GetPlayer(game, "Alice");
        var bob = GameServiceHelpers.GetPlayer(game, "Bob");

        service.RemoveInitialFrog(game.Id, alice.Token, new(1, 1));
        service.RemoveInitialFrog(game.Id, bob.Token, new(1, 2));

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.RemoveInitialFrog(game.Id, alice.Token, new(1, 3));
        });
    }

    [Fact]
    public void RemoveInitialFrog_AfterFirstJump_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        game.Board = new();

        Player alice = game.Players[0];
        Player bob = game.Players[1];

        alice.HasUsedInitialRemoval = true;
        bob.HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        service.MakeMove(game.Id, alice.Token,
            [new(3, 3), new(3, 5)]);

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.RemoveInitialFrog(game.Id, alice.Token, new(1, 1));
        });
    }
}