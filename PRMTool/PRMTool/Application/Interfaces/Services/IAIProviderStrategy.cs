using System.Threading.Tasks;

namespace Application.Interfaces.Services;

public interface IAIProviderStrategy
{
    string ProviderName { get; }
    Task<string> GenerateContentAsync(string apiKey, string prompt);
}
