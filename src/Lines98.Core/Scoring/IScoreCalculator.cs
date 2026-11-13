namespace Lines98.Core.Scoring;

public interface IScoreCalculator
{
    int CalculateScore(int removedBallCount);
}
