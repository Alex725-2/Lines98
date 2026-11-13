using Lines98.Core.Scoring;

namespace Lines98.Core.Tests;

public class ScorePerBallCalculatorTests
{
    [Theory]
    [InlineData(0)]
    [InlineData(5)]
    [InlineData(9)]
    [InlineData(81)]
    public void CalculateScore_ReturnsOnePointPerBall(int count)
    {
        IScoreCalculator calculator = new ScorePerBallCalculator();

        Assert.Equal(count, calculator.CalculateScore(count));
    }

    [Fact]
    public void CalculateScore_NegativeCount_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>("removedBallCount", () =>
            new ScorePerBallCalculator().CalculateScore(-1));
    }
}
