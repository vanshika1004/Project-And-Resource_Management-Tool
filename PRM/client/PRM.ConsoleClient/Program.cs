using PRM.ConsoleClient.Screens;
using PRM.ConsoleClient.Services;
using System.Threading.Tasks;

namespace PRM.ConsoleClient;

class Program
{
    static async Task Main(string[] args)
    {
        var apiClient = new ApiClient();
        var loginScreen = new LoginScreen(apiClient);

        await loginScreen.RunAsync();
    }
}
