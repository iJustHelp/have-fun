using Microsoft.AspNetCore.Components;

namespace HaveFun.Web;

public partial class Weather : ComponentBase
{
    private WeatherForecast[]? forecasts;

    protected override async Task OnInitializedAsync()
    {
        await Task.Delay(500);

        var startDate = DateOnly.FromDateTime(DateTime.Now);
        var summaries = new[]
        {
            "Freezing",
            "Bracing",
            "Chilly",
            "Cool",
            "Mild",
            "Warm",
            "Balmy",
            "Hot",
            "Sweltering",
            "Scorching"
        };

        forecasts = Enumerable.Range(1, 5).Select(index => new WeatherForecast
        {
            Date = startDate.AddDays(index),
            TemperatureC = Random.Shared.Next(-20, 55),
            Summary = summaries[Random.Shared.Next(summaries.Length)]
        }).ToArray();
    }

    private sealed class WeatherForecast
    {
        public DateOnly Date { get; init; }

        public int TemperatureC { get; init; }

        public string? Summary { get; init; }

        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
    }
}
