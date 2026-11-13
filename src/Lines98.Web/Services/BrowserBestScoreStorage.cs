using Microsoft.JSInterop;

namespace Lines98.Web.Services;

public sealed class BrowserBestScoreStorage(IJSRuntime js) : IBestScoreStorage
{
    public async ValueTask<int> ReadAsync()
    {
        try
        {
            var value = await js.InvokeAsync<string?>("lines98Storage.read");
            return int.TryParse(value, out var score) && score >= 0 ? score : 0;
        }
        catch (JSException) { return 0; }
        catch (OperationCanceledException) { return 0; }
    }

    public async ValueTask WriteAsync(int score)
    {
        try { await js.InvokeVoidAsync("lines98Storage.write", score.ToString(System.Globalization.CultureInfo.InvariantCulture)); }
        catch (JSException) { }
        catch (OperationCanceledException) { }
    }
}
