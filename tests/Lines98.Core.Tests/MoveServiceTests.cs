using Lines98.Core.Board;
using Lines98.Core.Game;
using Lines98.Core.Scoring;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class MoveServiceTests
{
    [Fact]
    public void Move_ReachableCell_MovesBallWithoutChangingOriginal()
    {
        var start = new Position(0, 0);
        var end = new Position(8, 8);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Orange));

        var result = new MoveService().Move(board, start, end);

        Assert.True(result.IsSuccess);
        Assert.Equal(MoveFailureReason.None, result.FailureReason);
        Assert.True(result.Board.GetCell(start).IsEmpty);
        Assert.Equal(BallColor.Orange, result.Board.GetCell(end).Color);
        Assert.Equal(80, result.Board.EmptyCount);
        Assert.Empty(result.RemovedPositions);
        Assert.Equal(0, result.ScoreEarned);
        Assert.Equal(BallColor.Orange, board.GetCell(start).Color);
        Assert.True(board.GetCell(end).IsEmpty);
        Assert.Equal(80, board.EmptyCount);
    }

    [Theory]
    [InlineData(MoveFailureReason.InvalidStart)]
    [InlineData(MoveFailureReason.InvalidEnd)]
    [InlineData(MoveFailureReason.SamePosition)]
    [InlineData(MoveFailureReason.EmptyStart)]
    [InlineData(MoveFailureReason.OccupiedEnd)]
    [InlineData(MoveFailureReason.NoPath)]
    public void Move_InvalidMove_ReturnsReasonAndOriginalBoard(MoveFailureReason reason)
    {
        var start = new Position(0, 0);
        var end = new Position(1, 1);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Red));
        switch (reason)
        {
            case MoveFailureReason.InvalidStart: start = new Position(-1, 0); break;
            case MoveFailureReason.InvalidEnd: end = new Position(0, 9); break;
            case MoveFailureReason.SamePosition: end = start; break;
            case MoveFailureReason.EmptyStart: board = new GameBoard(); break;
            case MoveFailureReason.OccupiedEnd:
                board = board.WithCell(end, Cell.WithBall(BallColor.Blue));
                break;
            case MoveFailureReason.NoPath:
                board = board.WithCell(new Position(0, 1), Cell.WithBall(BallColor.Blue))
                    .WithCell(new Position(1, 0), Cell.WithBall(BallColor.Blue));
                break;
        }

        var result = new MoveService().Move(board, start, end);

        Assert.False(result.IsSuccess);
        Assert.Equal(reason, result.FailureReason);
        Assert.Same(board, result.Board);
        Assert.Empty(result.RemovedPositions);
        Assert.Equal(0, result.ScoreEarned);
    }

    [Theory]
    [InlineData(0, 0, 0, 1, 5)]
    [InlineData(0, 0, 1, 0, 5)]
    [InlineData(0, 0, 1, 1, 5)]
    [InlineData(0, 8, 1, -1, 5)]
    [InlineData(0, 0, 0, 1, 7)]
    [InlineData(0, 0, 1, 0, 7)]
    [InlineData(0, 0, 1, 1, 7)]
    [InlineData(0, 8, 1, -1, 7)]
    [InlineData(0, 0, 0, 1, 4)]
    public void Move_CompletesSequence_RemovesOnlyLinesOfAtLeastFive(
        int row, int column, int rowStep, int columnStep, int length)
    {
        var board = new GameBoard();
        for (var index = 0; index < length - 1; index++)
        {
            board = board.WithCell(new Position(row + index * rowStep, column + index * columnStep),
                Cell.WithBall(BallColor.Red));
        }

        var end = new Position(row + (length - 1) * rowStep, column + (length - 1) * columnStep);
        var start = rowStep == 1 && columnStep == 0
            ? new Position(end.Row, end.Column + 1)
            : new Position(end.Row + 1, end.Column);
        board = board.WithCell(start, Cell.WithBall(BallColor.Red));
        var snapshot = Snapshot(board);

        var result = new MoveService().Move(board, start, end);

        Assert.True(result.IsSuccess);
        Assert.Equal(length >= 5 ? length : 0, result.ScoreEarned);
        Assert.Equal(result.ScoreEarned, result.RemovedPositions.Count);
        Assert.All(result.RemovedPositions, position => Assert.True(result.Board.GetCell(position).IsEmpty));
        Assert.Equal(length >= 5 ? 81 : 81 - length, result.Board.EmptyCount);
        Assert.Equal(length >= 5, result.Board.GetCell(end).IsEmpty);
        Assert.Equal(snapshot, Snapshot(board));
    }

    [Fact]
    public void Move_IntersectingAndSeparateLines_RemovesAllUniqueBallsSimultaneously()
    {
        var board = new GameBoard();
        for (var index = 0; index < 4; index++)
        {
            board = board.WithCell(new Position(4, index), Cell.WithBall(BallColor.Red))
                .WithCell(new Position(index, 4), Cell.WithBall(BallColor.Red));
        }

        for (var column = 0; column < 5; column++)
        {
            board = board.WithCell(new Position(8, column), Cell.WithBall(BallColor.Blue));
        }

        var start = new Position(5, 4);
        board = board.WithCell(start, Cell.WithBall(BallColor.Red))
            .WithCell(new Position(7, 8), Cell.WithBall(BallColor.Green));
        var snapshot = Snapshot(board);

        var result = new MoveService().Move(board, start, new Position(4, 4));

        Assert.True(result.IsSuccess);
        Assert.Equal(14, result.RemovedPositions.Count);
        Assert.Equal(14, result.RemovedPositions.Distinct().Count());
        Assert.Equal(14, result.ScoreEarned);
        Assert.Equal(80, result.Board.EmptyCount);
        Assert.Equal(BallColor.Green, result.Board.GetCell(new Position(7, 8)).Color);
        Assert.All(result.RemovedPositions, position => Assert.True(result.Board.GetCell(position).IsEmpty));
        Assert.Equal(snapshot, Snapshot(board));
    }

    [Fact]
    public void Move_UsesInjectedScoreCalculator()
    {
        var board = new GameBoard();
        for (var column = 0; column < 4; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        var start = new Position(1, 4);
        board = board.WithCell(start, Cell.WithBall(BallColor.Red));

        var result = new MoveService(new DoubleScoreCalculator()).Move(board, start, new Position(0, 4));

        Assert.Equal(5, result.RemovedPositions.Count);
        Assert.Equal(10, result.ScoreEarned);
    }

    [Fact]
    public void NullArguments_Throw()
    {
        Assert.Throws<ArgumentNullException>("scoreCalculator", () => new MoveService(null!));
        Assert.Throws<ArgumentNullException>("board", () =>
            new MoveService().Move(null!, new Position(0, 0), new Position(0, 1)));
    }

    private static Cell[] Snapshot(GameBoard board) => Enumerable.Range(0, 81)
        .Select(index => board.GetCell(new Position(index / 9, index % 9))).ToArray();

    private sealed class DoubleScoreCalculator : IScoreCalculator
    {
        public int CalculateScore(int removedBallCount) => removedBallCount * 2;
    }
}
