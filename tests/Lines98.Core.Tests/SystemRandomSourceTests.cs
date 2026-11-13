using Lines98.Core.Randomness;

namespace Lines98.Core.Tests;

public class SystemRandomSourceTests
{
    [Fact]
    public void Next_WithSameSeed_ReturnsSameValuesWithinBounds()
    {
        var first = new SystemRandomSource(42);
        var second = new SystemRandomSource(42);
        for (var index = 0; index < 100; index++)
        {
            var value = first.Next(7);
            Assert.InRange(value, 0, 6);
            Assert.Equal(value, second.Next(7));
        }
    }
}
