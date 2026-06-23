using PRM.Core.Entities;

namespace PRM.Application.Interfaces;

public interface IJwtTokenGenerator
{
    string GenerateToken(User user);
}
