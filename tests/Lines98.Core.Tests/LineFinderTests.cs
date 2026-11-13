using Lines98.Core.Board;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class LineFinderTests
{
    [Theory]
    [InlineData(0, 0, 0, 1, 5)]
    [InlineData(0, 0, 1, 0, 5)]
    [InlineData(0, 0, 1, 1, 5)]
    [InlineData(0, 8, 1, -1, 5)]
    [InlineData(0, 0, 0, 1, 9)]
    [InlineData(0, 0, 1, 0, 9)]
    [InlineData(0, 0, 1, 1, 9)]
    [InlineData(0, 8, 1, -1, 9)]
    public void FindLines_FindsEntireSequence(int row, int column, int rowStep, int columnStep, int length)
    {
        var board = new GameBoard();
        var expected = new List<Position>();
        for (var index = 0; index < length; index++)
        {
            var position = new Position(row + index * rowStep, column + index * columnStep);
            expected.Add(position);
            board = board.WithCell(position, Cell.WithBall(BallColor.Red));
        }

        var matches = LineFinder.FindLines(board);

        Assert.Equal(expected.Count, matches.Count);
        Assert.All(expected, position => Assert.Contains(position, matches));
        Assert.All(expected, position => Assert.Equal(BallColor.Red, board.GetCell(position).Color));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void FindLines_GapOrDifferentColorBreaksSequence(bool differentColor)
    {
        var board = new GameBoard();
        for (var column = 0; column < 9; column++)
        {
            board = board.WithCell(new Position(0, column), Cell.WithBall(BallColor.Red));
        }

        board = board.WithCell(new Position(0, 4), differentColor ? Cell.WithBall(BallColor.Blue) : Cell.Empty);

        Assert.Empty(LineFinder.FindLines(board));
    }

    [Fact]
    public void FindLines_EmptyBoard_ReturnsEmpty()
    {
        Assert.Empty(LineFinder.FindLines(new GameBoard()));
    }

    [Fact]
    public void FindLines_NullBoard_Throws()
    {
        Assert.Throws<ArgumentNullException>("board", () => LineFinder.FindLines(null!));
    }
}
