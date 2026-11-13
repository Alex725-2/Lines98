using Lines98.Core.Board;

namespace Lines98.Core.Tests;

public class PositionTests
{
    [Theory]
    [InlineData(0, 0, true)]
    [InlineData(8, 8, true)]
    [InlineData(4, 5, true)]
    [InlineData(-1, 0, false)]
    [InlineData(0, 9, false)]
    [InlineData(9, 9, false)]
    public void IsWithinBoard_ReturnsExpectedResult(int row, int column, bool expected)
    {
        var position = new Position(row, column);

        Assert.Equal(expected, position.IsWithinBoard);
    }
}
