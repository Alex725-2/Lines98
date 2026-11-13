using Lines98.Core.Board;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Game;

public sealed class GameState
{
    /// <summary>Creates a settled state; boards with unresolved lines are rejected.</summary>
    public GameState(GameBoard board, int score, IEnumerable<BallColor> nextColors)
    {
        ArgumentNullException.ThrowIfNull(board);
        ArgumentNullException.ThrowIfNull(nextColors);
        ArgumentOutOfRangeException.ThrowIfNegative(score);
        if (LineFinder.FindLines(board).Count > 0)
        {
            throw new ArgumentException("Board must not contain unresolved lines.", nameof(board));
        }

        var colors = nextColors.ToArray();
        if (colors.Length != 3 || colors.Any(color => !Enum.IsDefined(color)))
        {
            throw new ArgumentException("Exactly three valid ball colors are required.", nameof(nextColors));
        }

        Board = board;
        Score = score;
        NextColors = Array.AsReadOnly(colors);
    }

    public GameBoard Board { get; }

    public int Score { get; }

    public IReadOnlyList<BallColor> NextColors { get; }

    public bool IsGameOver => Board.IsFull;
}
