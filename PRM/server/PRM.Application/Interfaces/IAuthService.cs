using System.Threading.Tasks;
using PRM.Application.DTOs;

namespace PRM.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
    Task ChangePasswordAsync(ChangePasswordRequest request);
}
