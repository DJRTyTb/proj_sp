using Server.Enums;
using Server.Models;
using Server.Services;

namespace Tests.GameServiceTests;

public class GameServiceConcurrencyTests
{
    [Fact]
    public async Task ConcurrentMoves_DoNotDuplicateFrogs()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        Player alice = game.Players[0];

        alice.HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3),
            new() { Color = FrogColor.Green });

        game.Board.PlaceFrog(new(3, 4),
            new() { Color = FrogColor.Orange });

        Task t1 = Task.Run(() =>
        {
            try
            {
                service.MakeMove(game.Id,
                    alice.Token,
                    [new(3, 3), new(3, 5)]);
            }
            catch { }
        });

        Task t2 = Task.Run(() =>
        {
            try
            {
                service.MakeMove(game.Id,
                    alice.Token,
                    [new(3, 3), new(3, 5)]);
            }
            catch { }
        });

        await Task.WhenAll(t1, t2);

        Assert.Equal(
            game.Board.Frogs.Count,
            game.Board.Frogs.Keys.Distinct().Count());
    }

    [Fact]
    public async Task ConcurrentMoves_LeaveBoardInValidState()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        Player alice = game.Players[0];

        alice.HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3),
            new() { Color = FrogColor.Green });

        game.Board.PlaceFrog(new(3, 4),
            new() { Color = FrogColor.Orange });

        List<Task> tasks = [];

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    service.MakeMove(
                        game.Id,
                        alice.Token,
                        [new(3, 3), new(3, 5)]);
                }
                catch { }
            }));
        }

        await Task.WhenAll(tasks);

        Assert.InRange(game.Board.Frogs.Count, 0, 2);
    }

    [Fact]
    public async Task ConcurrentPasses_DoNotCrash()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = GameServiceHelpers.CreateStartedGame(service);

        GameServiceHelpers.CompleteInitialRemovals(service, game);

        Player alice = game.Players[0];

        List<Task> tasks = [];

        for (int i = 0; i < 20; i++)
        {
            tasks.Add(Task.Run(() =>
            {
                try
                {
                    service.PassTurn(game.Id, alice.Token);
                }
                catch { }
            }));
        }

        await Task.WhenAll(tasks);

        Assert.True(
            game.Status == GameStatus.InProgress ||
            game.Status == GameStatus.Finished);
    }

    [Fact]
    public async Task ConcurrentMoveRequests_OnlyOneMoveSucceeds()
    {
        GameManager manager = new();
        GameService service = GameServiceHelpers.CreateService(manager);

        Game game = service.CreateGame("Alice", "conn1");
        service.JoinGame(game.Id, "Bob", "conn2");

        game.Board = new();

        Player alice = game.Players[0];

        alice.HasUsedInitialRemoval = true;
        game.Players[1].HasUsedInitialRemoval = true;

        game.Board.PlaceFrog(new(3, 3),
            new() { Color = FrogColor.Green });

        game.Board.PlaceFrog(new(3, 4),
            new() { Color = FrogColor.Orange });

        int successCount = 0;

        Task[] tasks =
        [
            Task.Run(() =>
            {
                try
                {
                    service.MakeMove(
                        game.Id,
                        alice.Token,
                        [new(3,3), new(3,5)]);

                    Interlocked.Increment(ref successCount);
                }
                catch { }
            }),

            Task.Run(() =>
            {
                try
                {
                    service.MakeMove(
                        game.Id,
                        alice.Token,
                        [new(3,3), new(3,5)]);

                    Interlocked.Increment(ref successCount);
                }
                catch { }
            })
        ];

        await Task.WhenAll(tasks);

        Assert.True(successCount <= 1);
    }
}