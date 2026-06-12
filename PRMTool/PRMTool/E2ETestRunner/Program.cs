using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

class Program
{
    private static readonly HttpClient _httpClient = new HttpClient { BaseAddress = new Uri("http://localhost:5144") };

    static async Task Main(string[] args)
    {
        Console.WriteLine("Resetting password for Riya Mehta...");
        
        var loginData = new { Username = "admin", Password = "Admin@5678" };
        var response = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData);
        var result = await response.Content.ReadFromJsonAsync<LoginResult>();
        _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", result.Token);

        var resetResponse = await _httpClient.PutAsJsonAsync("/api/users/7/reset-password", "Riya@1234");
        var resBody = await resetResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"Status: {resetResponse.StatusCode}, Body: {resBody}");

        Console.WriteLine("Testing Login for Riya Mehta...");
        
        var loginData2 = new { Username = "riyamehta", Password = "Riya@1234" };
        var response2 = await _httpClient.PostAsJsonAsync("/api/auth/login", loginData2);
        
        var body2 = await response2.Content.ReadAsStringAsync();
        Console.WriteLine($"Status Code: {response2.StatusCode} ({(int)response2.StatusCode})");
        Console.WriteLine($"Response Body: {body2}");
    }

    class LoginResult
    {
        public string Token { get; set; }
    }
}
