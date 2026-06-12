using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class AllocationApiClient : ApiClientBase
{
    public static async Task<List<AllocationDto>> GetAllAsync()
    {
        return await GetAsync<List<AllocationDto>>("allocations") ?? new List<AllocationDto>();
    }

    public static async Task<AllocationDto> CreateAsync(CreateAllocationDto dto)
    {
        var result = await PostAsync<AllocationDto>("allocations", dto);
        return result ?? throw new Exception("Empty response from server.");
    }

    public static async Task EndAllocationAsync(int allocationId)
    {
        await PutAsync($"allocations/{allocationId}/end", new { });
    }
}

public class AllocationDto
{
    public int Id { get; set; }
    public int EmployeeId { get; set; }
    public string EmployeeName { get; set; } = string.Empty;
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = string.Empty;
    public decimal UtilizationPercent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}

public class CreateAllocationDto
{
    public int EmployeeId { get; set; }
    public int ProjectId { get; set; }
    public decimal UtilizationPercent { get; set; }
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
}
