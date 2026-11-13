using Lines98.Core.Board;

namespace Lines98.Core.Tests;

public class CellTests
{
    [Fact]
    public void Empty_CreatesEmptyCell()
    {
        var cell = Cell.Empty;

        Assert.True(cell.IsEmpty);
        Assert.Null(cell.Color);
    }

    [Fact]
    public void WithBall_Red_CreatesOccupiedCell()
    {
        var cell = Cell.WithBall(BallColor.Red);

        Assert.False(cell.IsEmpty);
        Assert.Equal(BallColor.Red, cell.Color);
    }

    [Theory]
    [InlineData(BallColor.Red)]
    [InlineData(BallColor.Blue)]
    [InlineData(BallColor.Green)]
    [InlineData(BallColor.Yellow)]
    [InlineData(BallColor.Purple)]
    [InlineData(BallColor.Cyan)]
    [InlineData(BallColor.Orange)]
    public void WithBall_CreatesOccupiedCellForEachColor(BallColor color)
    {
        var cell = Cell.WithBall(color);

        Assert.False(cell.IsEmpty);
        Assert.Equal(color, cell.Color);
    }

    [Fact]
    public void Default_IsEmpty()
    {
        var cell = default(Cell);

        Assert.True(cell.IsEmpty);
        Assert.Null(cell.Color);
    }

    [Fact]
    public void EmptyCells_AreEqual()
    {
        Assert.Equal(Cell.Empty, Cell.Empty);
    }

    [Fact]
    public void CellsWithSameColor_AreEqual()
    {
        Assert.Equal(Cell.WithBall(BallColor.Blue), Cell.WithBall(BallColor.Blue));
    }

    [Fact]
    public void CellsWithDifferentColors_AreNotEqual()
    {
        Assert.NotEqual(Cell.WithBall(BallColor.Red), Cell.WithBall(BallColor.Blue));
    }
}
