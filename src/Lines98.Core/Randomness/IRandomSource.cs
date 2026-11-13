namespace Lines98.Core.Randomness;

public interface IRandomSource
{
    // Returns a value in [0, exclusiveMax).
    int Next(int exclusiveMax);
}
