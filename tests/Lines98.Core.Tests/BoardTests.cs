using Lines98.Core.Board;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class BoardTests
{
    [Fact]
    public void Constructor_CreatesEmptyNineByNineBoard()
    {
        var board = new GameBoard();

        Assert.Equal(9, GameBoard.Size);
        Assert.Equal(81, board.EmptyCount);
        Assert.False(board.IsFull);
        var expected = new List<Position>();
        for (var row = 0; row < 9; row++)
        {
            for (var column = 0; column < 9; column++)
            {
                var position = new Position(row, column);
                expected.Add(position);
                Assert.Equal(Cell.Empty, board.GetCell(position));
            }
        }

        Assert.Equal(expected, board.EmptyPositions);
    }

    [Theory]
    [InlineData(0, 0)]
    [InlineData(0, 8)]
    [InlineData(8, 0)]
    [InlineData(8, 8)]
    [InlineData(4, 5)]
    public void WithCell_ChangesOnlySelectedCellAndPreservesOriginal(int row, int column)
    {
        var board = new GameBoard();
        var position = new Position(row, column);
        var cell = Cell.WithBall(BallColor.Red);

        var updated = board.WithCell(position, cell);

        Assert.NotSame(board, updated);
        Assert.Equal(cell, updated.GetCell(position));
        Assert.Equal(80, updated.EmptyCount);
        Assert.DoesNotContain(position, updated.EmptyPositions);
        Assert.False(updated.IsFull);
        Assert.Equal(81, board.EmptyCount);
        Assert.Contains(position, board.EmptyPositions);
        foreach (var originalPosition in board.EmptyPositions)
        {
            Assert.Equal(Cell.Empty, board.GetCell(originalPosition));
            if (originalPosition != position)
            {
                Assert.Equal(Cell.Empty, updated.GetCell(originalPosition));
            }
        }
    }

    [Fact]
    public void WithCell_CanReplaceAndClearBallWithoutChangingPreviousBoards()
    {
        var position = new Position(4, 4);
        var red = new GameBoard().WithCell(position, Cell.WithBall(BallColor.Red));
        var blue = red.WithCell(position, Cell.WithBall(BallColor.Blue));
        var cleared = blue.WithCell(position, Cell.Empty);

        Assert.Equal(BallColor.Red, red.GetCell(position).Color);
        Assert.Equal(BallColor.Blue, blue.GetCell(position).Color);
        Assert.Equal(80, red.EmptyCount);
        Assert.Equal(80, blue.EmptyCount);
        Assert.Equal(Cell.Empty, cleared.GetCell(position));
        Assert.Equal(81, cleared.EmptyCount);
        Assert.Contains(position, cleared.EmptyPositions);
    }

    [Fact]
    public void IsFull_WhenAllCellsOccupied_ReturnsTrue()
    {
        var board = new GameBoard();
        foreach (var position in board.EmptyPositions)
        {
            board = board.WithCell(position, Cell.WithBall(BallColor.Green));
        }

        Assert.True(board.IsFull);
        Assert.Equal(0, board.EmptyCount);
        Assert.Empty(board.EmptyPositions);

        var cleared = board.WithCell(new Position(8, 8), Cell.Empty);

        Assert.False(cleared.IsFull);
        Assert.Equal(new[] { new Position(8, 8) }, cleared.EmptyPositions);
        Assert.True(board.IsFull);
    }

    [Theory]
    [InlineData(-1, 0)]
    [InlineData(0, -1)]
    [InlineData(9, 0)]
    [InlineData(0, 9)]
    [InlineData(int.MinValue, int.MaxValue)]
    public void InvalidPosition_GetCellAndWithCellThrow(int row, int column)
    {
        var board = new GameBoard();
        var position = new Position(row, column);

        Assert.Throws<ArgumentOutOfRangeException>("position", () => board.GetCell(position));
        Assert.Throws<ArgumentOutOfRangeException>("position", () => board.WithCell(position, Cell.Empty));
    }

    [Fact]
    public void EmptyPositions_CannotBeModifiedThroughCollectionInterface()
    {
        var board = new GameBoard();
        var positions = Assert.IsAssignableFrom<IList<Position>>(board.EmptyPositions);

        Assert.Throws<NotSupportedException>(() => positions[0] = new Position(8, 8));
        Assert.Equal(new Position(0, 0), board.EmptyPositions[0]);
        Assert.Equal(81, board.EmptyCount);
    }
}
