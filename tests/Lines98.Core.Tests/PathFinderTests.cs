using Lines98.Core.Board;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class PathFinderTests
{
    [Theory]
    [InlineData(0, 0, 0, 8)]
    [InlineData(8, 8, 0, 8)]
    public void FindPath_StraightPath_ReturnsShortestPath(int startRow, int startColumn, int endRow, int endColumn)
    {
        var start = new Position(startRow, startColumn);
        var end = new Position(endRow, endColumn);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Red));

        var path = PathFinder.FindPath(board, start, end);

        AssertValidPath(board, start, end, path, 9);
    }

    [Fact]
    public void FindPath_Obstacle_ReturnsShortestDetour()
    {
        var start = new Position(0, 0);
        var end = new Position(0, 2);
        var board = new GameBoard()
            .WithCell(start, Cell.WithBall(BallColor.Red))
            .WithCell(new Position(0, 1), Cell.WithBall(BallColor.Blue));

        var path = PathFinder.FindPath(board, start, end);

        AssertValidPath(board, start, end, path, 5);
    }

    [Fact]
    public void FindPath_CompleteWall_ReturnsNull()
    {
        var start = new Position(4, 0);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Red));
        for (var row = 0; row < GameBoard.Size; row++)
        {
            board = board.WithCell(new Position(row, 4), Cell.WithBall(BallColor.Blue));
        }

        Assert.Null(PathFinder.FindPath(board, start, new Position(4, 8)));
    }

    [Fact]
    public void FindPath_OnlyDiagonalExit_ReturnsNull()
    {
        var start = new Position(0, 0);
        var board = new GameBoard()
            .WithCell(start, Cell.WithBall(BallColor.Red))
            .WithCell(new Position(0, 1), Cell.WithBall(BallColor.Blue))
            .WithCell(new Position(1, 0), Cell.WithBall(BallColor.Blue));

        Assert.Null(PathFinder.FindPath(board, start, new Position(1, 1)));
    }

    [Fact]
    public void FindPath_OccupiedEnd_ReturnsNull()
    {
        var start = new Position(0, 0);
        var end = new Position(0, 1);
        var board = new GameBoard()
            .WithCell(start, Cell.WithBall(BallColor.Red))
            .WithCell(end, Cell.WithBall(BallColor.Blue));

        Assert.Null(PathFinder.FindPath(board, start, end));
    }

    [Fact]
    public void FindPath_EmptyStart_ReturnsNull()
    {
        Assert.Null(PathFinder.FindPath(new GameBoard(), new Position(0, 0), new Position(0, 1)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FindPath_SameEndpoints_ReturnsNull(bool occupied)
    {
        var position = new Position(4, 4);
        var board = new GameBoard();
        if (occupied)
        {
            board = board.WithCell(position, Cell.WithBall(BallColor.Red));
        }

        Assert.Null(PathFinder.FindPath(board, position, position));
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void FindPath_InvalidCoordinates_ThrowsForEitherEndpoint(int row, int column)
    {
        var invalid = new Position(row, column);
        var valid = new Position(0, 0);
        var board = new GameBoard();

        Assert.Throws<ArgumentOutOfRangeException>("start", () => PathFinder.FindPath(board, invalid, valid));
        Assert.Throws<ArgumentOutOfRangeException>("end", () => PathFinder.FindPath(board, valid, invalid));
    }

    [Fact]
    public void FindPath_NullBoard_Throws()
    {
        Assert.Throws<ArgumentNullException>("board", () =>
            PathFinder.FindPath(null!, new Position(0, 0), new Position(0, 1)));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FindPath_DoesNotChangeBoard(bool blocked)
    {
        var start = new Position(0, 0);
        var board = new GameBoard().WithCell(start, Cell.WithBall(BallColor.Red));
        if (blocked)
        {
            board = board.WithCell(new Position(0, 1), Cell.WithBall(BallColor.Blue))
                .WithCell(new Position(1, 0), Cell.WithBall(BallColor.Blue));
        }

        var snapshot = new List<Cell>();
        for (var row = 0; row < GameBoard.Size; row++)
        {
            for (var column = 0; column < GameBoard.Size; column++)
            {
                snapshot.Add(board.GetCell(new Position(row, column)));
            }
        }

        var emptyPositions = board.EmptyPositions.ToArray();
        var path = PathFinder.FindPath(board, start, new Position(8, 8));

        Assert.Equal(blocked, path is null);
        for (var row = 0; row < GameBoard.Size; row++)
        {
            for (var column = 0; column < GameBoard.Size; column++)
            {
                Assert.Equal(snapshot[row * GameBoard.Size + column], board.GetCell(new Position(row, column)));
            }
        }

        Assert.Equal(emptyPositions, board.EmptyPositions);
        Assert.Equal(emptyPositions.Length, board.EmptyCount);
        Assert.False(board.IsFull);
    }

    private static void AssertValidPath(GameBoard board, Position start, Position end,
        IReadOnlyList<Position>? path, int expectedCount)
    {
        Assert.NotNull(path);
        Assert.Equal(expectedCount, path.Count);
        Assert.Equal(start, path[0]);
        Assert.Equal(end, path[^1]);
        Assert.Equal(path.Count, path.Distinct().Count());
        for (var index = 1; index < path.Count; index++)
        {
            Assert.True(path[index].IsWithinBoard);
            Assert.True(board.GetCell(path[index]).IsEmpty);
            Assert.Equal(1, Math.Abs(path[index].Row - path[index - 1].Row)
                + Math.Abs(path[index].Column - path[index - 1].Column));
        }
    }
}
