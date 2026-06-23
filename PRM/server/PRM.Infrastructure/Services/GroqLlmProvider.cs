using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PRM.Infrastructure.Services;

public class GroqLlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GroqLlmProvider(HttpClient httpClient, string apiKey)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_apiKey))
        {
            return "AI feature unavailable: API Key not configured.";
        }

        var requestBody = new
        {
            model = "openai/gpt-oss-20b",
            messages = new[]
            {
                new { role = "user", content = prompt }
            },
            temperature = 0.2
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions");
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
        request.Content = content;

        try
        {
            var response = await _httpClient.SendAsync(request);
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return $"AI provider returned an error: {response.StatusCode} - {err}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            var answer = document.RootElement
                .GetProperty("choices")[0]
                .GetProperty("message")
                .GetProperty("content")
                .GetString();

            return answer ?? "AI returned an empty response.";
        }
        catch (Exception)
        {
            return "AI feature temporarily unavailable due to a network or configuration error.";
        }
    }
}
