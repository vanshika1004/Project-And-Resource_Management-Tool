using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class EmployeeApiClient : ApiClientBase
{
    public static async Task<List<EmployeeDto>> GetAllAsync()
    {
        return await GetAsync<List<EmployeeDto>>("employees") ?? new List<EmployeeDto>();
    }

    public static async Task<List<EmployeeDto>> GetMyTeamAsync()
    {
        return await GetAsync<List<EmployeeDto>>("employees/team") ?? new List<EmployeeDto>();
    }

    public static async Task<EmployeeDto?> GetByIdAsync(int id)
    {
        return await GetAsync<EmployeeDto>($"employees/{id}");
    }

    public static async Task<EmployeeDto> CreateAsync(CreateEmployeeDto dto)
    {
        var result = await PostAsync<EmployeeDto>("employees", dto);
        return result ?? throw new Exception("Empty response from server.");
    }

    public static async Task UpdateAsync(int id, UpdateEmployeeDto dto)
    {
        await PutAsync($"employees/{id}", dto);
    }

    public static async Task DeleteAsync(int id)
    {
        await ApiClientBase.DeleteAsync($"employees/{id}");
    }
}

public class EmployeeDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
    public int TotalExperienceYears { get; set; }
    public int? ManagerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public int UserId { get; set; }
    public bool IsActive { get; set; }
}

public class CreateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public DateTime DateOfJoining { get; set; }
    public int TotalExperienceYears { get; set; }
    public int? ManagerId { get; set; }
}

public class UpdateEmployeeDto
{
    public string FullName { get; set; } = string.Empty;
    public string Designation { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public int TotalExperienceYears { get; set; }
    public int? ManagerId { get; set; }
    public string Status { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
