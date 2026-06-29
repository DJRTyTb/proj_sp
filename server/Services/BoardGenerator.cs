using Server.Enums;
using Server.Models;
using System.Runtime.InteropServices;

namespace Server.Services;

public class BoardGenerator(IRandomProvider randomProvider)
{
    private readonly IRandomProvider _randomProvider = randomProvider;

    public Board Generate()
    {
        Board board = new();

        List<Position> positions = [];

        for (int row = 1; row <= 6; row++)
            for (int col = 1; col <= 6; col++)
                positions.Add(new(row, col));

        _randomProvider.Shuffle(CollectionsMarshal.AsSpan(positions));

        for (int i = 0; i < 18; i++)
            board.PlaceFrog(positions[i], new() { Color = FrogColor.Green });

        for (int i = 18; i < 36; i++)
            board.PlaceFrog(positions[i], new() { Color = FrogColor.Orange });

        return board;
    }
}