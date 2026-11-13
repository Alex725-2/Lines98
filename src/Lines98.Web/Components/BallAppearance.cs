using Lines98.Core.Board;

namespace Lines98.Web.Components;

public static class BallAppearance
{
    public static string Name(BallColor color) => color switch
    {
        BallColor.Red => "Красный",
        BallColor.Blue => "Синий",
        BallColor.Green => "Зелёный",
        BallColor.Yellow => "Жёлтый",
        BallColor.Purple => "Фиолетовый",
        BallColor.Cyan => "Голубой",
        BallColor.Orange => "Оранжевый",
        _ => throw new ArgumentOutOfRangeException(nameof(color))
    };

    public static string CssClass(BallColor color) => $"ball-{color.ToString().ToLowerInvariant()}";
}
