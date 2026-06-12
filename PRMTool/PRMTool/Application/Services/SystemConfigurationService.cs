using Application.DTOs.System;
using Application.Interfaces.Repositories;
using Application.Interfaces.Services;
using Domain.Entities;

namespace Application.Services;

public class SystemConfigurationService : ISystemConfigurationService
{
    private readonly ISystemConfigurationRepository _configRepository;

    public SystemConfigurationService(ISystemConfigurationRepository configRepository)
    {
        _configRepository = configRepository;
    }

    public async Task<SystemConfigurationDto> GetConfigurationAsync()
    {
        var config = await _configRepository.GetAsync();

        if (config == null)
        {
            config = new SystemConfiguration
            {
                LLMProvider = "Google Gemini",
                ApiKey = "",
                SchedulerInterval = 4,
                MaxWeeklyHours = 40
            };
            await _configRepository.AddAsync(config);
            await _configRepository.SaveChangesAsync();
        }

        return new SystemConfigurationDto
        {
            LLMProvider = config.LLMProvider,
            ApiKey = config.ApiKey,
            SchedulerInterval = config.SchedulerInterval,
            MaxWeeklyHours = config.MaxWeeklyHours
        };
    }

    public async Task UpdateConfigurationAsync(SystemConfigurationDto dto)
    {
        var config = await _configRepository.GetAsync();

        if (config == null)
        {
            config = new SystemConfiguration
            {
                LLMProvider = dto.LLMProvider,
                ApiKey = dto.ApiKey,
                SchedulerInterval = dto.SchedulerInterval,
                MaxWeeklyHours = dto.MaxWeeklyHours
            };
            await _configRepository.AddAsync(config);
        }
        else
        {
            config.LLMProvider = dto.LLMProvider;
            config.ApiKey = dto.ApiKey;
            config.SchedulerInterval = dto.SchedulerInterval;
            config.MaxWeeklyHours = dto.MaxWeeklyHours;
            _configRepository.Update(config);
        }

        await _configRepository.SaveChangesAsync();
    }
}
