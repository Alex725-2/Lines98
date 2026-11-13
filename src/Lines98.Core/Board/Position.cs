namespace Lines98.Core.Board;

public readonly record struct Position(int Row, int Column)
{
    public bool IsWithinBoard => Row is >= 0 and < 9 && Column is >= 0 and < 9;
}
