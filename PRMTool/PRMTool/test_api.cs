using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

class Program
{
    static async Task Main()
    {
        var client = new HttpClient { BaseAddress = new Uri("http://localhost:5144/api/") };
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        
        try {
            Console.WriteLine("1. Logging in as admin...");
            var loginRes = await client.PostAsJsonAsync("auth/login", new { Username = "admin", Password = "Admin@5678" });
            var content = await loginRes.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {loginRes.StatusCode}");
            Console.WriteLine($"Body: {content}");
            
            if (!loginRes.IsSuccessStatusCode) return;
            
            var adminData = JsonSerializer.Deserialize<JsonElement>(content);
            var token = adminData.GetProperty("token").GetString();
            client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            
            var uniqueId = Guid.NewGuid().ToString().Substring(0, 4);
            var newUsername = "test" + uniqueId;
            Console.WriteLine($"\n2. Creating new user: {newUsername}");
            var createRes = await client.PostAsJsonAsync("users", new {
                FullName = "Test User " + uniqueId,
                Email = $"test{uniqueId}@example.com",
                Username = newUsername,
                TemporaryPassword = "TempPassword@1",
                Role = 1
            });
            var createContent = await createRes.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {createRes.StatusCode}");
            Console.WriteLine($"Body: {createContent}");
            
            if (!createRes.IsSuccessStatusCode) return;
            
            var createData = JsonSerializer.Deserialize<JsonElement>(createContent, options);
            var newUserId = createData.GetProperty("userId").GetInt32();
            
            client.DefaultRequestHeaders.Authorization = null;
            
            Console.WriteLine($"\n3. Logging in as new user: {newUsername}");
            var login2Res = await client.PostAsJsonAsync("auth/login", new { Username = newUsername, Password = "TempPassword@1" });
            var login2Content = await login2Res.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {login2Res.StatusCode}");
            Console.WriteLine($"Body: {login2Content}");
            
            Console.WriteLine($"\n4. Changing password for new user...");
            var cpRes = await client.PostAsJsonAsync("auth/change-password", new {
                UserId = newUserId,
                CurrentPassword = "TempPassword@1",
                NewPassword = "NewPassword@123"
            });
            Console.WriteLine($"Status: {cpRes.StatusCode}");
            Console.WriteLine($"Body: {await cpRes.Content.ReadAsStringAsync()}");
            
            Console.WriteLine($"\n5. Logging in again with new password...");
            var login3Res = await client.PostAsJsonAsync("auth/login", new { Username = newUsername, Password = "NewPassword@123" });
            var login3Content = await login3Res.Content.ReadAsStringAsync();
            Console.WriteLine($"Status: {login3Res.StatusCode}");
            Console.WriteLine($"Body: {login3Content}");
            
        }
        catch(Exception ex)
        {
            Console.WriteLine(ex);
        }
    }
}
