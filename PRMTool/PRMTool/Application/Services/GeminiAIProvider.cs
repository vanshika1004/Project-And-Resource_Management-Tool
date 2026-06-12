using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces.Services;

namespace Application.Services;

public class GeminiAIProvider : IAIProviderStrategy
{
    private readonly HttpClient _httpClient;

    public GeminiAIProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderName => "Google Gemini";

    public async Task<string> GenerateContentAsync(string apiKey, string prompt)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return "Mock response: You need to set an API Key first. But since we are testing, this is a simulated AI response.";

        var url = $"https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent?key={apiKey}";
        var payload = new
        {
            contents = new[]
            {
                new { parts = new[] { new { text = prompt } } }
            }
        };

        var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        var response = await _httpClient.PostAsync(url, content);
        
        if (!response.IsSuccessStatusCode)
        {
            return $"Error calling Gemini: {response.StatusCode}";
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        try
        {
            return doc.RootElement.GetProperty("candidates")[0].GetProperty("content").GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
        }
        catch
        {
            return "Failed to parse Gemini response.";
        }
    }
}
