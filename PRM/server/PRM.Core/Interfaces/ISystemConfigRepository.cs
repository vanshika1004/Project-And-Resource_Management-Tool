using System.Threading.Tasks;

namespace PRM.Core.Interfaces;

public interface ISystemConfigRepository
{
    Task<string?> GetValueAsync(string key);
}
