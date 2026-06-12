using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class EmployeeSkillApiClient : ApiClientBase
{
    public static async Task<List<EmployeeSkillDto>> GetByEmployeeIdAsync(int employeeId)
    {
        return await GetAsync<List<EmployeeSkillDto>>($"EmployeeSkills/employee/{employeeId}") ?? new List<EmployeeSkillDto>();
    }

    public static async Task AddAsync(CreateEmployeeSkillDto dto)
    {
        await PostAsync<object>($"EmployeeSkills", dto);
    }

    public static async Task RemoveAsync(int id)
    {
        await DeleteAsync($"EmployeeSkills/{id}");
    }
}

public class EmployeeSkillDto
{
    public int Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ProficiencyLevel { get; set; } = string.Empty;
}

public class CreateEmployeeSkillDto
{
    public int EmployeeId { get; set; }
    public int SkillId { get; set; }
    public string ProficiencyLevel { get; set; } = string.Empty;
}
