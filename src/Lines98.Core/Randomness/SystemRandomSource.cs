namespace Lines98.Core.Randomness;

public sealed class SystemRandomSource : IRandomSource
{
    private readonly Random random;

    public SystemRandomSource() : this(Random.Shared)
    {
    }

    public SystemRandomSource(int seed) : this(new Random(seed))
    {
    }

    private SystemRandomSource(Random random)
    {
        this.random = random;
    }

    public int Next(int exclusiveMax) => random.Next(exclusiveMax);
}
