namespace Lines98.Core.Scoring;

public sealed class ScorePerBallCalculator : IScoreCalculator
{
    public int CalculateScore(int removedBallCount)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(removedBallCount);
        return removedBallCount;
    }
}
