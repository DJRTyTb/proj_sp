using Server.Enums;
using Server.Models;

namespace Server.Services;

public class MoveValidator
{
    public bool IsMoveValid(Board board, List<Position> path, FrogColor playerColor)
    {
        if (path.Count < 2) return false;

        Board simulationBoard = CloneBoard(board);

        Frog? frog = simulationBoard.GetFrog(path[0]);

        if (frog == null || frog.Color != playerColor) return false;

        for (int i = 0; i < path.Count - 1; i++)
        {
            Position from = path[i];
            Position to = path[i + 1];

            if (!IsSingleJumpValid(simulationBoard, from, to)) return false;

            Position jumped = GetJumpedPosition(from, to);

            simulationBoard.RemoveFrog(jumped);
            simulationBoard.MoveFrog(from, to);
        }

        return true;
    }

    public bool IsSingleJumpValid(Board board, Position from, Position to)
    {
        if (!board.IsInsideBoard(from) || !board.IsInsideBoard(to)) return false;
        if (!board.IsOccupied(from)) return false;
        if (board.IsOccupied(to)) return false;

        int rowDiff = to.Row - from.Row;
        int colDiff = to.Col - from.Col;

        if (Math.Abs(rowDiff) > 2 || Math.Abs(colDiff) > 2) return false;
        if (Math.Abs(rowDiff) != 2 && rowDiff != 0) return false;
        if (Math.Abs(colDiff) != 2 && colDiff != 0) return false;
        if (rowDiff == 0 && colDiff == 0) return false;

        bool straight = rowDiff == 0 || colDiff == 0;
        bool diagonal = Math.Abs(rowDiff) == Math.Abs(colDiff);

        if (!straight && !diagonal) return false;

        Position jumped = GetJumpedPosition(from, to);

        return board.IsOccupied(jumped);
    }

    public Position GetJumpedPosition(Position from, Position to)
    {
        int row = from.Row + (to.Row - from.Row) / 2;
        int col = from.Col + (to.Col - from.Col) / 2;

        return new(row, col);
    }

    public bool HasAnyValidMove(Board board, FrogColor color)
    {
        foreach ((Position position, Frog frog) in board.Frogs)
        {
            if (frog.Color != color) continue;
            if (HasValidMoveForFrog(board, position)) return true;
        }

        return false;
    }

    public bool HasValidMoveForFrog(Board board, Position position)
    {
        int[] directions = [-2, 0, 2];

        foreach (int rowOffset in directions)
        {
            foreach (int colOffset in directions)
            {
                if (rowOffset == 0 && colOffset == 0) continue;

                Position target = new(position.Row + rowOffset, position.Col + colOffset);

                if (IsSingleJumpValid(board, position, target)) return true;
            }
        }

        return false;
    }

    private static Board CloneBoard(Board source)
    {
        Board clone = new();

        foreach ((Position position, Frog frog) in source.Frogs)
            clone.PlaceFrog(position, frog);

        return clone;
    }
}