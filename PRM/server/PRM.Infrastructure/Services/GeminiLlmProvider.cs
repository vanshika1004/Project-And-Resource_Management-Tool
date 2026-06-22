using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace PRM.Infrastructure.Services;

public class GeminiLlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GeminiLlmProvider(HttpClient httpClient, string apiKey)
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
            contents = new[]
            {
                new 
                { 
                    parts = new[] 
                    {
                        new { text = prompt }
                    }
                }
            }
        };

        var json = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        string url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-2.0-flash:generateContent?key={_apiKey}";
        var request = new HttpRequestMessage(HttpMethod.Post, url);
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
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString();

            return answer ?? "AI returned an empty response.";
        }
        catch (Exception)
        {
            return "AI feature temporarily unavailable due to a network or configuration error.";
        }
    }
}
