using Application.DTOs.System;

namespace Application.Interfaces.Services;

public interface ISystemConfigurationService
{
    Task<SystemConfigurationDto> GetConfigurationAsync();
    Task UpdateConfigurationAsync(SystemConfigurationDto dto);
}
