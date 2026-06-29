using Server.Enums;
using Server.Models;
using Server.Services;

namespace Tests.GameServiceTests;

public class GameServicePassTests
{
    [Fact]
    public void PassTurn_SwitchesTurn()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        GameServiceHelpers.CompleteInitialRemovals(service, game);

        Player alice = GameServiceHelpers.GetPlayer(game, "Alice");

        service.PassTurn(game.Id, alice.Token);

        Assert.Equal("Bob", game.CurrentPlayer.Name);
    }

    [Fact]
    public void PassTurn_IncrementsConsecutivePasses()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        GameServiceHelpers.CompleteInitialRemovals(service, game);

        Player alice = GameServiceHelpers.GetPlayer(game, "Alice");

        service.PassTurn(game.Id, alice.Token);

        Assert.Equal(1, game.ConsecutivePasses);
    }

    [Fact]
    public void PassTurn_TwoPassesFinishGame()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        GameServiceHelpers.CompleteInitialRemovals(service, game);

        Player alice = GameServiceHelpers.GetPlayer(game, "Alice");
        Player bob = GameServiceHelpers.GetPlayer(game, "Bob");

        service.PassTurn(game.Id, alice.Token);
        service.PassTurn(game.Id, bob.Token);

        Assert.Equal(GameStatus.Finished, game.Status);
    }

    [Fact]
    public void SuccessfulMove_ResetsConsecutivePasses()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        Player alice = game.Players[0];
        Player bob = game.Players[1];

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 6), new() { Color = FrogColor.Green });

        service.PassTurn(game.Id, alice.Token);
        service.MakeMove(game.Id, bob.Token, [new(3, 3), new(3, 5)]);

        Assert.Equal(0, game.ConsecutivePasses);
    }
}