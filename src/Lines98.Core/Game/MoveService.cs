using Lines98.Core.Board;
using Lines98.Core.Scoring;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Game;

public sealed class MoveService
{
    private readonly IScoreCalculator scoreCalculator;

    public MoveService() : this(new ScorePerBallCalculator())
    {
    }

    public MoveService(IScoreCalculator scoreCalculator)
    {
        ArgumentNullException.ThrowIfNull(scoreCalculator);
        this.scoreCalculator = scoreCalculator;
    }

    public MoveResult Move(GameBoard board, Position start, Position end)
    {
        ArgumentNullException.ThrowIfNull(board);
        var failure = GetFailureReason(board, start, end);
        if (failure != MoveFailureReason.None)
        {
            return new MoveResult(board, failure, Array.Empty<Position>(), 0);
        }

        var moved = board.WithCell(start, Cell.Empty).WithCell(end, board.GetCell(start));
        var removedPositions = LineFinder.FindLines(moved);
        foreach (var position in removedPositions)
        {
            moved = moved.WithCell(position, Cell.Empty);
        }

        var score = scoreCalculator.CalculateScore(removedPositions.Count);
        return new MoveResult(moved, MoveFailureReason.None, removedPositions, score);
    }

    private static MoveFailureReason GetFailureReason(GameBoard board, Position start, Position end)
    {
        if (!start.IsWithinBoard) return MoveFailureReason.InvalidStart;
        if (!end.IsWithinBoard) return MoveFailureReason.InvalidEnd;
        if (start == end) return MoveFailureReason.SamePosition;
        if (board.GetCell(start).IsEmpty) return MoveFailureReason.EmptyStart;
        if (!board.GetCell(end).IsEmpty) return MoveFailureReason.OccupiedEnd;
        return PathFinder.FindPath(board, start, end) is null
            ? MoveFailureReason.NoPath
            : MoveFailureReason.None;
    }
}
