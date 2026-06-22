using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace PRM.ConsoleClient.Services;

public class ApiClient
{
    private readonly HttpClient _http;
    private readonly string _baseUrl = "http://localhost:5018/api";

    private readonly JsonSerializerOptions _jsonOptions;

    public ApiClient()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);
        _jsonOptions.Converters.Add(new JsonStringEnumConverter());
    }

    private void AddAuthorization()
    {
        if (AuthState.IsAuthenticated)
        {
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", AuthState.Token);
        }
        else
        {
            _http.DefaultRequestHeaders.Authorization = null;
        }
    }

    public async Task<T?> GetAsync<T>(string endpoint)
    {
        AddAuthorization();
        var response = await _http.GetAsync($"{_baseUrl}/{endpoint}");
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<T>(_jsonOptions);
    }

    public async Task<TResponse?> PostAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AddAuthorization();
        var response = await _http.PostAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
        await EnsureSuccessAsync(response);
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<TResponse>(content, _jsonOptions);
    }

    public async Task PostAsync<TRequest>(string endpoint, TRequest data)
    {
        AddAuthorization();
        var response = await _http.PostAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
        await EnsureSuccessAsync(response);
    }

    public async Task<TResponse?> PutAsync<TRequest, TResponse>(string endpoint, TRequest data)
    {
        AddAuthorization();
        var response = await _http.PutAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
        await EnsureSuccessAsync(response);
        return await response.Content.ReadFromJsonAsync<TResponse>(_jsonOptions);
    }

    public async Task PutAsync<TRequest>(string endpoint, TRequest data)
    {
        AddAuthorization();
        var response = await _http.PutAsJsonAsync($"{_baseUrl}/{endpoint}", data, _jsonOptions);
        await EnsureSuccessAsync(response);
    }

    public async Task DeleteAsync(string endpoint)
    {
        AddAuthorization();
        var response = await _http.DeleteAsync($"{_baseUrl}/{endpoint}");
        await EnsureSuccessAsync(response);
    }

    private async Task EnsureSuccessAsync(HttpResponseMessage response)
    {
        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            string errorMessage = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}";
            try
            {
                var errorObj = JsonSerializer.Deserialize<JsonElement>(content);
                if (errorObj.TryGetProperty("error", out var errorProp))
                {
                    errorMessage = errorProp.GetString() ?? errorMessage;
                }
                else if (errorObj.TryGetProperty("message", out var msgProp))
                {
                    errorMessage = msgProp.GetString() ?? errorMessage;
                }
            }
            catch
            {
                // If not JSON, use raw content if not empty
                if (!string.IsNullOrWhiteSpace(content))
                    errorMessage += $": {content}";
            }
            throw new ApiException(errorMessage);
        }
    }
}

public class ApiException : Exception
{
    public ApiException(string message) : base(message) { }
}
