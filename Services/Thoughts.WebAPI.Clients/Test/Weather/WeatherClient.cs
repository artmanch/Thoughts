﻿using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Thoughts.WebAPI.Clients.Test.Weather;

public class WeatherClient : IWeatherService
{
    private readonly HttpClient _httpClient;

    public WeatherClient(HttpClient httpClient) => _httpClient = httpClient;

    public async Task<IEnumerable<WeatherInfo>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var result = await _httpClient
            .GetFromJsonAsync<IEnumerable<WeatherInfo>>("api/test/weather", cancellationToken)
            .ConfigureAwait(false);

        return result ?? Enumerable.Empty<WeatherInfo>();
    }

    public IEnumerable<WeatherInfo> GetAll() => GetAllAsync().GetAwaiter().GetResult();
}

public record WeatherInfo(DateTime Date, string? Summary)
{
    [JsonPropertyName("TemperatureC")]
    public int Temperature { get; init; }
}