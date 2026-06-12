using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using ConsoleClient.Storage;

namespace ConsoleClient.ApiClients;

public abstract class ApiClientBase
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:5144/api/")
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    protected static HttpClient Client
    {
        get
        {
            if (SessionManager.IsLoggedIn)
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue(
                        "Bearer", SessionManager.Token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }

            return _httpClient;
        }
    }

    protected static async Task<T?> GetAsync<T>(string url)
    {
        var response = await Client.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content
            .ReadFromJsonAsync<T>(_jsonOptions);
    }

    protected static async Task<T?> PostAsync<T>(
        string url, object payload)
    {
        var response = await Client.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(errorContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msg))
                {
                    throw new Exception(msg.GetString());
                }
            }
            catch (JsonException)
            {
                // Not JSON, use raw content
            }

            throw new Exception(
                $"Request failed ({(int)response.StatusCode}): "
                + errorContent);
        }

        return await response.Content
            .ReadFromJsonAsync<T>(_jsonOptions);
    }

    protected static async Task PostAsync(
        string url, object payload)
    {
        var response = await Client.PostAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync();

            try
            {
                using var doc = JsonDocument.Parse(errorContent);
                var root = doc.RootElement;

                if (root.TryGetProperty("message", out var msg))
                {
                    throw new Exception(msg.GetString());
                }
            }
            catch (JsonException)
            {
                // Not JSON, use raw content
            }

            throw new Exception(
                $"Request failed ({(int)response.StatusCode}): "
                + errorContent);
        }
    }

    protected static async Task PutAsync(
        string url, object payload)
    {
        var response = await Client.PutAsJsonAsync(url, payload);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync();

            throw new Exception(
                $"Request failed ({(int)response.StatusCode}): "
                + errorContent);
        }
    }

    protected static async Task DeleteAsync(string url)
    {
        var response = await Client.DeleteAsync(url);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content
                .ReadAsStringAsync();

            throw new Exception(
                $"Request failed ({(int)response.StatusCode}): "
                + errorContent);
        }
    }
}
