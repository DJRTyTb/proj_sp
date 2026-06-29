using Server.Enums;
using Server.Models;
using Server.Services;
using Tests.Fakes;

namespace Tests.BoardGeneratorTests;

public class BoardGeneratorTests
{
    [Fact]
    public void Generate_CreatesExactly36Frogs()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        Assert.Equal(36, board.Frogs.Count);
    }

    [Fact]
    public void Generate_Creates18GreenFrogs()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        int greenCount = board.Frogs.Values.Count(f => f.Color == FrogColor.Green);

        Assert.Equal(18, greenCount);
    }

    [Fact]
    public void Generate_Creates18OrangeFrogs()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        int orangeCount = board.Frogs.Values.Count(f => f.Color == FrogColor.Orange);

        Assert.Equal(18, orangeCount);
    }

    [Fact]
    public void Generate_AllFrogsAreInsidePlayableArea()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        Assert.All(board.Frogs.Keys, position =>
        {
            Assert.True(position.Row >= 1 && position.Row <= 6);
            Assert.True(position.Col >= 1 && position.Col <= 6);
        });
    }

    [Fact]
    public void Generate_AllPlayableCellsAreFilled()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        for (int row = 1; row <= 6; row++)
        {
            for (int col = 1; col <= 6; col++)
            {
                Assert.True(board.IsOccupied(new(row, col)));
            }
        }
    }

    [Fact]
    public void Generate_DoesNotPlaceFrogsInSwamp()
    {
        BoardGenerator generator = new(new RandomProvider());

        Board board = generator.Generate();

        foreach ((Position position, Frog _) in board.Frogs)
        {
            Assert.False(board.IsSwamp(position));
        }
    }

    [Fact]
    public void Generate_WithFakeRandomProvider_GeneratesDeterministicBoard()
    {
        BoardGenerator generator = new(new FakeRandomProvider());

        Board board = generator.Generate();

        Assert.Equal(FrogColor.Green, board.GetFrog(new(1, 1))!.Color);
        Assert.Equal(FrogColor.Green, board.GetFrog(new(3, 6))!.Color);

        Assert.Equal(FrogColor.Orange, board.GetFrog(new(4, 1))!.Color);
        Assert.Equal(FrogColor.Orange, board.GetFrog(new(6, 6))!.Color);
    }

    [Fact]
    public void Generate_MultipleBoards_AreNotAlwaysEqual()
    {
        BoardGenerator generator = new(new RandomProvider());

        HashSet<string> layouts = [];

        for (int i = 0; i < 10; i++)
        {
            Board board = generator.Generate();

            string layout = string.Join(";",
                board.Frogs
                    .OrderBy(x => x.Key.Row)
                    .ThenBy(x => x.Key.Col)
                    .Select(x => x.Value.Color));

            layouts.Add(layout);
        }

        Assert.True(layouts.Count > 1);
    }
}