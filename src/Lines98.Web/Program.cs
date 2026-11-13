using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Lines98.Web;
using Lines98.Core.Game;
using Lines98.Core.Randomness;
using Lines98.Core.Scoring;
using Lines98.Web.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<IRandomSource, SystemRandomSource>();
builder.Services.AddScoped<IScoreCalculator, ScorePerBallCalculator>();
builder.Services.AddScoped<GameEngine>();
builder.Services.AddScoped<GameSession>();
builder.Services.AddScoped<IBestScoreStorage, BrowserBestScoreStorage>();
builder.Services.AddScoped<BestScoreService>();

await builder.Build().RunAsync();
