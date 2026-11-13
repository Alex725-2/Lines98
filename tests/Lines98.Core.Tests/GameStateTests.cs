using Lines98.Core.Board;
using Lines98.Core.Game;
using GameBoard = Lines98.Core.Board.Board;

namespace Lines98.Core.Tests;

public class GameStateTests
{
    [Fact]
    public void Constructor_CopiesPreviewAndExposesReadOnlyCollection()
    {
        BallColor[] colors = [BallColor.Red, BallColor.Blue, BallColor.Green];
        var state = new GameState(new GameBoard(), 12, colors);
        colors[0] = BallColor.Orange;

        Assert.Equal(BallColor.Red, state.NextColors[0]);
        var list = Assert.IsAssignableFrom<IList<BallColor>>(state.NextColors);
        Assert.Throws<NotSupportedException>(() => list[0] = BallColor.Orange);
        Assert.Equal(12, state.Score);
        Assert.False(state.IsGameOver);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(2)]
    [InlineData(4)]
    public void Constructor_WrongPreviewLength_Throws(int count)
    {
        Assert.Throws<ArgumentException>("nextColors", () =>
            new GameState(new GameBoard(), 0, Enumerable.Repeat(BallColor.Red, count)));
    }

    [Theory]
    [InlineData(0, 1, 5)]
    [InlineData(1, 0, 6)]
    [InlineData(1, 1, 5)]
    [InlineData(1, -1, 5)]
    public void Constructor_UnresolvedLine_Throws(int rowStep, int columnStep, int length)
    {
        var board = new GameBoard();
        for (var index = 0; index < length; index++)
        {
            board = board.WithCell(new Position(index * rowStep, columnStep < 0 ? 8 - index : index * columnStep),
                Cell.WithBall(BallColor.Red));
        }

        Assert.Throws<ArgumentException>("board", () =>
            new GameState(board, 0, [BallColor.Red, BallColor.Blue, BallColor.Green]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Constructor_FullBoard_OnlyAcceptsBoardWithoutLines(bool lines)
    {
        var board = new GameBoard();
        for (var row = 0; row < 9; row++)
        {
            for (var column = 0; column < 9; column++)
            {
                board = board.WithCell(new Position(row, column),
                    Cell.WithBall(lines ? BallColor.Red : (BallColor)((row + 2 * column) % 7)));
            }
        }

        BallColor[] colors = [BallColor.Red, BallColor.Blue, BallColor.Green];
        if (lines)
        {
            Assert.Throws<ArgumentException>("board", () => new GameState(board, 0, colors));
        }
        else
        {
            Assert.True(new GameState(board, 0, colors).IsGameOver);
        }
    }

    [Fact]
    public void Constructor_InvalidArguments_Throw()
    {
        BallColor[] colors = [BallColor.Red, BallColor.Blue, BallColor.Green];
        Assert.Throws<ArgumentNullException>("board", () => new GameState(null!, 0, colors));
        Assert.Throws<ArgumentNullException>("nextColors", () => new GameState(new GameBoard(), 0, null!));
        Assert.Throws<ArgumentOutOfRangeException>("score", () => new GameState(new GameBoard(), -1, colors));
        Assert.Throws<ArgumentException>("nextColors", () =>
            new GameState(new GameBoard(), 0, [BallColor.Red, BallColor.Blue, (BallColor)99]));
    }
}
