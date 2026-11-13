namespace Lines98.Core.Board;

public enum BallColor
{
    Red,
    Blue,
    Green,
    Yellow,
    Purple,
    Cyan,
    Orange
}

public readonly record struct Cell
{
    private Cell(BallColor? color)
    {
        if (color is { } value && !Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(nameof(color), color, "Unknown ball color.");
        }

        Color = color;
    }

    public BallColor? Color { get; }

    public bool IsEmpty => Color is null;

    public static Cell Empty => default;

    public static Cell WithBall(BallColor color) => new(color);
}
