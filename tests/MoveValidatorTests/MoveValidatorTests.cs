using Server.Enums;
using Server.Models;
using Server.Services;

namespace Tests.MoveValidatorTests;

public class MoveValidatorTests
{
    private readonly MoveValidator _validator = new();

    [Fact]
    public void IsSingleJumpValid_HorizontalJump_ReturnsTrue()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(3, 5));

        Assert.True(result);
    }

    [Fact]
    public void IsSingleJumpValid_VerticalJump_ReturnsTrue()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(4, 3), new() { Color = FrogColor.Orange });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(5, 3));

        Assert.True(result);
    }

    [Fact]
    public void IsSingleJumpValid_DiagonalJump_ReturnsTrue()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(4, 4), new() { Color = FrogColor.Orange });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(5, 5));

        Assert.True(result);
    }

    [Fact]
    public void IsSingleJumpValid_WithoutMiddleFrog_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(3, 5));

        Assert.False(result);
    }

    [Fact]
    public void IsSingleJumpValid_TargetOccupied_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });
        board.PlaceFrog(new(3, 5), new() { Color = FrogColor.Green });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(3, 5));

        Assert.False(result);
    }

    [Fact]
    public void IsSingleJumpValid_TooLongJump_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(3, 7));

        Assert.False(result);
    }

    [Fact]
    public void IsSingleJumpValid_OneCellMove_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(3, 4));

        Assert.False(result);
    }

    [Fact]
    public void IsSingleJumpValid_OutsideBoard_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(1, 0), new() { Color = FrogColor.Orange });

        bool result = _validator.IsSingleJumpValid(board, new(1, 1), new(1, -1));

        Assert.False(result);
    }

    [Fact]
    public void IsMoveValid_ValidChain_ReturnsTrue()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(1, 2), new() { Color = FrogColor.Orange });
        board.PlaceFrog(new(2, 3), new() { Color = FrogColor.Orange });

        List<Position> path =
        [
            new(1, 1),
            new(1, 3),
            new(3, 3)
        ];

        bool result = _validator.IsMoveValid(board, path, FrogColor.Green);

        Assert.True(result);
    }

    [Fact]
    public void IsMoveValid_InvalidChainAfterRemoval_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(1, 2), new() { Color = FrogColor.Orange });

        List<Position> path =
        [
            new(1, 1),
            new(1, 3),
            new(1, 1)
        ];

        bool result = _validator.IsMoveValid(board, path, FrogColor.Green);

        Assert.False(result);
    }

    [Fact]
    public void IsMoveValid_WrongColor_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Orange });
        board.PlaceFrog(new(1, 2), new() { Color = FrogColor.Green });

        List<Position> path =
        [
            new(1, 1),
            new(1, 3)
        ];

        bool result = _validator.IsMoveValid(board, path, FrogColor.Green);

        Assert.False(result);
    }

    [Fact]
    public void IsMoveValid_PathTooShort_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Green });

        List<Position> path =
        [
            new(1, 1)
        ];

        bool result = _validator.IsMoveValid(board, path, FrogColor.Green);

        Assert.False(result);
    }

    [Fact]
    public void HasAnyValidMove_PlayerHasMove_ReturnsTrue()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(3, 4), new() { Color = FrogColor.Orange });

        bool result = _validator.HasAnyValidMove(board, FrogColor.Green);

        Assert.True(result);
    }

    [Fact]
    public void HasAnyValidMove_PlayerHasNoMoves_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });

        bool result = _validator.HasAnyValidMove(board, FrogColor.Green);

        Assert.False(result);
    }

    [Fact]
    public void HasAnyValidMove_OnlyOpponentHasMoves_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(1, 1), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(1, 2), new() { Color = FrogColor.Orange });
        board.PlaceFrog(new(1, 3), new() { Color = FrogColor.Green });

        bool result = _validator.HasAnyValidMove(board, FrogColor.Green);

        Assert.False(result);
    }

    [Fact]
    public void IsSingleJumpValid_NotStraightLine_ReturnsFalse()
    {
        Board board = new();

        board.PlaceFrog(new(3, 3), new() { Color = FrogColor.Green });
        board.PlaceFrog(new(4, 4), new() { Color = FrogColor.Orange });

        bool result = _validator.IsSingleJumpValid(board, new(3, 3), new(5, 4));

        Assert.False(result);
    }
}