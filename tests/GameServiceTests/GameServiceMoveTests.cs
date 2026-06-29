using Server.Enums;
using Server.Models;
using Server.Services;

namespace Tests.GameServiceTests;

public class GameServiceMoveTests
{
    [Fact]
    public void MakeMove_RemovesJumpedFrog()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);

        Assert.False(game.Board.IsOccupied(new(3, 4)));
    }

    [Fact]
    public void MakeMove_MovesFrogToDestination()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);

        Assert.False(game.Board.IsOccupied(new(3, 3)));
        Assert.True(game.Board.IsOccupied(new(3, 5)));
    }

    [Fact]
    public void MakeMove_SetsLastJumpPlayer()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);

        Assert.Equal(alice, game.LastJumpPlayer);
    }

    [Fact]
    public void MakeMove_SwitchesTurn()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(3, 6), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);

        Assert.Equal("Bob", game.CurrentPlayer.Name);
    }

    [Fact]
    public void MakeMove_InvalidMove_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });

        Player alice = game.Players[0];

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);
        });
    }

    [Fact]
    public void MakeMove_PlayerMovesOpponentFrog_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Green });

        Player alice = game.Players[0];

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);
        });
    }

    [Fact]
    public void MakeMove_NotCurrentPlayer_Throws()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Green });

        Player bob = game.Players[1];

        Assert.Throws<InvalidOperationException>(() =>
        {
            service.MakeMove(game.Id, bob.Token, [new(3, 3), new(3, 5)]);
        });
    }

    [Fact]
    public void MakeMove_IntoSwamp_RemovesFrog()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(1, 5), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(1, 6), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(1, 5), new(1, 7)]);

        Assert.False(game.Board.IsOccupied(new(1, 7)));
    }

    [Fact]
    public void MakeMove_FromSwampToBoard_FrogRemains()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 5), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(2, 6), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(1, 6), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token, [new(3, 5), new(1, 7), new(1, 5)]);

        Assert.True(game.Board.IsOccupied(new(1, 5)));
    }

    [Fact]
    public void MakeMove_ChainMove_RemovesAllJumpedFrogs()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        game.Players[0].HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 1), new() { Color = FrogColor.Green });
        game.Board.PlaceFrog(new(3, 2), new() { Color = FrogColor.Orange });
        game.Board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        Player alice = game.Players[0];

        service.MakeMove(game.Id, alice.Token,
        [
            new(3, 1),
            new(3, 3),
            new(3, 5)
        ]);

        Assert.False(game.Board.IsOccupied(new(3, 2)));
        Assert.False(game.Board.IsOccupied(new(3, 4)));
    }

    [Fact]
    public void GameFinish_LastJumpPlayerWins()
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

        service.MakeMove(game.Id, alice.Token, [new(3, 3), new(3, 5)]);

        Assert.Equal(GameStatus.Finished, game.Status);
        Assert.Equal(alice, game.LastJumpPlayer);
    }
}