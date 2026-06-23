using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace PRM.Infrastructure.Services;

public class CustomLlmProvider
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;
    private readonly string _endpointUrl;
    private readonly string _model;

    public CustomLlmProvider(HttpClient httpClient, string apiKey, string endpointUrl, string model)
    {
        _httpClient = httpClient;
        _apiKey = apiKey;
        _endpointUrl = endpointUrl;
        _model = string.IsNullOrWhiteSpace(model) ? "gemma" : model;
    }

    public async Task<string> GenerateTextAsync(string prompt)
    {
        if (string.IsNullOrWhiteSpace(_endpointUrl))
        {
            return "AI feature unavailable: Custom endpoint URL not configured.";
        }

        // Mask key for diagnostics (show first 4 and last 4 chars)
        string maskedKey = MaskKey(_apiKey);

        // Auto-detect format from the endpoint URL
        bool isOllamaFormat = _endpointUrl.Contains("/api/generate", StringComparison.OrdinalIgnoreCase)
                           || _endpointUrl.Contains("/api/chat", StringComparison.OrdinalIgnoreCase)
                           || _endpointUrl.Contains(":11434", StringComparison.OrdinalIgnoreCase);

        string json;
        if (isOllamaFormat)
        {
            // Ollama native format
            var requestBody = new
            {
                model = _model,
                prompt = prompt,
                stream = false,
                options = new { temperature = 0.1 }
            };
            json = JsonSerializer.Serialize(requestBody);
        }
        else
        {
            // OpenAI-compatible chat completions format (default for organization gateways)
            var requestBody = new
            {
                model = _model,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = 0.2
            };
            json = JsonSerializer.Serialize(requestBody);
        }

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, _endpointUrl);
        
        // Set API key as a custom header 'apikey' per user curl command
        if (!string.IsNullOrWhiteSpace(_apiKey))
        {
            request.Headers.TryAddWithoutValidation("apikey", _apiKey);
        }
        
        request.Content = content;

        try
        {
            // Use a 5-minute timeout for LLM inference (models can be slow, especially large ones)
            using var cts = new CancellationTokenSource(TimeSpan.FromMinutes(5));
            var response = await _httpClient.SendAsync(request, cts.Token);
            
            if (!response.IsSuccessStatusCode)
            {
                var err = await response.Content.ReadAsStringAsync();
                return $"[Custom Provider → {_endpointUrl}] [Key: {maskedKey}] AI error: {response.StatusCode} - {err}";
            }

            var responseJson = await response.Content.ReadAsStringAsync();
            using var document = JsonDocument.Parse(responseJson);
            
            // Try Ollama format first: {"response": "..."}
            if (document.RootElement.TryGetProperty("response", out JsonElement responseElement))
            {
                return responseElement.GetString() ?? "AI returned an empty response.";
            }
            
            // OpenAI chat completions format: {"choices": [{"message": {"content": "..."}}]}
            if (document.RootElement.TryGetProperty("choices", out JsonElement choicesElement) && choicesElement.GetArrayLength() > 0)
            {
                var firstChoice = choicesElement[0];
                if (firstChoice.TryGetProperty("message", out JsonElement messageElement) && 
                    messageElement.TryGetProperty("content", out JsonElement contentElement))
                {
                    return contentElement.GetString() ?? "AI returned an empty response.";
                }
                if (firstChoice.TryGetProperty("text", out JsonElement textElement))
                {
                    return textElement.GetString() ?? "AI returned an empty response.";
                }
            }

            // Gemini format: {"candidates": [{"content": {"parts": [{"text": "..."}]}}]}
            if (document.RootElement.TryGetProperty("candidates", out JsonElement candidatesElement) && candidatesElement.GetArrayLength() > 0)
            {
                var firstCandidate = candidatesElement[0];
                if (firstCandidate.TryGetProperty("content", out JsonElement candContent) &&
                    candContent.TryGetProperty("parts", out JsonElement partsElement) &&
                    partsElement.GetArrayLength() > 0)
                {
                    var firstPart = partsElement[0];
                    if (firstPart.TryGetProperty("text", out JsonElement textElem))
                    {
                        return textElem.GetString() ?? "AI returned an empty response.";
                    }
                }
            }

            return $"AI returned an unexpected response format. Raw: {responseJson[..Math.Min(200, responseJson.Length)]}";
        }
        catch (OperationCanceledException)
        {
            return $"[Custom Provider → {_endpointUrl}] AI request timed out after 5 minutes. The model may be overloaded or the prompt is too large.";
        }
        catch (Exception ex)
        {
            return $"[Custom Provider → {_endpointUrl}] AI error: {ex.Message}";
        }
    }

    private static string MaskKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) return "(empty)";
        if (key.Length <= 8) return new string('*', key.Length);
        return $"{key[..4]}...{key[^4..]}";
    }
}
