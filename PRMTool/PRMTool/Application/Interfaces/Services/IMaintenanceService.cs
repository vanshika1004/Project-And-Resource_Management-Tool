using System.Threading.Tasks;

namespace Application.Interfaces.Services;

public interface IMaintenanceService
{
    Task RunMaintenanceAsync();
}
