using Lines98.Web.Services;

namespace Lines98.Core.Tests;

public class BestScoreServiceTests
{
    [Fact]
    public async Task Initialize_LoadsRecordAndUpdateOnlyWritesHigherScore()
    {
        var storage = new MemoryStorage { Score = 12 };
        var service = new BestScoreService(storage);
        await service.InitializeAsync();
        Assert.Equal(12, service.BestScore);
        await service.UpdateAsync(10);
        await service.UpdateAsync(12);
        Assert.Equal(0, storage.Writes);
        await service.UpdateAsync(20);
        Assert.Equal(20, service.BestScore);
        Assert.Equal(20, storage.Score);
        Assert.Equal(1, storage.Writes);
        await service.UpdateAsync(0);
        Assert.Equal(20, service.BestScore);
        var reloaded = new BestScoreService(storage);
        await reloaded.InitializeAsync();
        Assert.Equal(20, reloaded.BestScore);
    }

    [Fact]
    public async Task UnavailableStorage_KeepsRecordInMemory()
    {
        var service = new BestScoreService(new UnavailableStorage());
        await service.InitializeAsync();
        await service.UpdateAsync(15);
        await service.UpdateAsync(0);
        Assert.Equal(15, service.BestScore);
    }

    private sealed class MemoryStorage : IBestScoreStorage
    {
        public int Score { get; set; }
        public int Writes { get; private set; }
        public ValueTask<int> ReadAsync() => ValueTask.FromResult(Score);
        public ValueTask WriteAsync(int score) { Score = score; Writes++; return ValueTask.CompletedTask; }
    }

    // Mirrors the browser adapter's graceful fallback when storage is denied.
    private sealed class UnavailableStorage : IBestScoreStorage
    {
        public ValueTask<int> ReadAsync() => ValueTask.FromResult(0);
        public ValueTask WriteAsync(int score) => ValueTask.CompletedTask;
    }
}
