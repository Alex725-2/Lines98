using Lines98.Core.Board;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Game;

public enum MoveFailureReason
{
    None,
    InvalidStart,
    InvalidEnd,
    SamePosition,
    EmptyStart,
    OccupiedEnd,
    NoPath,
    /// <summary>The game has already ended.</summary>
    GameOver
}

public sealed class MoveResult
{
    internal MoveResult(GameBoard board, MoveFailureReason failureReason,
        IReadOnlyList<Position> removedPositions, int scoreEarned)
    {
        Board = board;
        FailureReason = failureReason;
        RemovedPositions = removedPositions;
        ScoreEarned = scoreEarned;
    }

    public GameBoard Board { get; }

    public bool IsSuccess => FailureReason == MoveFailureReason.None;

    public MoveFailureReason FailureReason { get; }

    public IReadOnlyList<Position> RemovedPositions { get; }

    public int ScoreEarned { get; }
}
