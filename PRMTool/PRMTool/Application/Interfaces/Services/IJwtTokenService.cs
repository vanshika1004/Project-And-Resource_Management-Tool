using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.Interfaces.Services
{
    public interface IJwtTokenService
    {
        string GenerateToken(User user);
    }
}
