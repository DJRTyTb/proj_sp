using Server.Enums;
using Server.Services;

namespace Tests.GameServiceTests;

public class GameServiceCreateJoinTests
{
    [Fact]
    public void CreateGame_CreatesSinglePlayerGame()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        Assert.Single(game.Players);
        Assert.Equal("Alice", game.Players[0].Name);
        Assert.Equal(FrogColor.Green, game.Players[0].Color);
    }

    [Fact]
    public void JoinGame_AddsSecondPlayer()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        service.JoinGame(game.Id, "Bob", "conn2");

        Assert.Equal(2, game.Players.Count);
        Assert.Equal("Bob", game.Players[1].Name);
        Assert.Equal(FrogColor.Orange, game.Players[1].Color);
    }

    [Fact]
    public void JoinGame_StartsGame()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        service.JoinGame(game.Id, "Bob", "conn2");

        Assert.Equal(GameStatus.InProgress, game.Status);
    }

    [Fact]
    public void JoinGame_FillsBoard()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        service.JoinGame(game.Id, "Bob", "conn2");

        Assert.Equal(36, game.Board.Frogs.Count);
    }

    [Fact]
    public void JoinGame_ThirdPlayer_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        service.JoinGame(game.Id, "Bob", "conn2");

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.JoinGame(game.Id, "Charlie", "conn3");
        });
    }

    [Fact]
    public void JoinGame_DuplicateName_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        var game = service.CreateGame("Alice", "conn1");

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.JoinGame(game.Id, "Alice", "conn2");
        });
    }
}