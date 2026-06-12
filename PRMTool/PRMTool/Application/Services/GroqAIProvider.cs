using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Application.Interfaces.Services;

namespace Application.Services;

public class GroqAIProvider : IAIProviderStrategy
{
    private readonly HttpClient _httpClient;

    public GroqAIProvider(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public string ProviderName => "Groq";

    public async Task<string> GenerateContentAsync(string apiKey, string prompt)
    {
        if (string.IsNullOrWhiteSpace(apiKey)) return "Mock response: You need to set an API Key first. But since we are testing, this is a simulated AI response.";

        var url = "https://api.groq.com/openai/v1/chat/completions";
        var payload = new
        {
            model = "llama3-8b-8192",
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
        request.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

        var response = await _httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            return $"Error calling Groq: {response.StatusCode}";
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        try
        {
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";
        }
        catch
        {
            return "Failed to parse Groq response.";
        }
    }
}
