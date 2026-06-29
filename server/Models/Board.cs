namespace Server.Models;

public class Board
{
    private readonly Dictionary<Position, Frog> _frogs = [];

    public IReadOnlyDictionary<Position, Frog> Frogs => _frogs;

    public bool IsOccupied(Position position) => _frogs.ContainsKey(position);

    public Frog? GetFrog(Position position) => _frogs.GetValueOrDefault(position);

    public void PlaceFrog(Position position, Frog frog) => _frogs[position] = frog;

    public void RemoveFrog(Position position) => _frogs.Remove(position);

    public void MoveFrog(Position from, Position to)
    {
        Frog frog = _frogs[from];

        _frogs.Remove(from);
        _frogs[to] = frog;
    }

    public bool IsInsideBoard(Position position) => position.Row >= 0 && position.Row < 8 &&
                                                     position.Col >= 0 && position.Col < 8;

    public bool IsSwamp(Position position) => position.Row == 0 ||
                                              position.Row == 7 ||
                                              position.Col == 0 ||
                                              position.Col == 7;

    public bool IsPlayable(Position position) => !IsSwamp(position);
}