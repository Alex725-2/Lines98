namespace Lines98.Web.Services;

public interface IBestScoreStorage
{
    ValueTask<int> ReadAsync();
    ValueTask WriteAsync(int score);
}

public sealed class BestScoreService(IBestScoreStorage storage)
{
    private Task? initialization;
    public int BestScore { get; private set; }

    public Task InitializeAsync() => initialization ??= LoadAsync();

    private async Task LoadAsync()
    {
        BestScore = Math.Max(BestScore, Math.Max(0, await storage.ReadAsync()));
    }

    public async Task UpdateAsync(int score)
    {
        await InitializeAsync();
        if (score <= BestScore) return;
        BestScore = score;
        await storage.WriteAsync(BestScore);
    }
}
