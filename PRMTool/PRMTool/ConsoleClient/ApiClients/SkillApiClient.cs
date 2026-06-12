using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConsoleClient.ApiClients;

public class SkillApiClient : ApiClientBase
{
    public static async Task<List<SkillDto>> GetAllAsync()
    {
        return await GetAsync<List<SkillDto>>("Skills") ?? new List<SkillDto>();
    }

    public static async Task<int> CreateAsync(CreateSkillDto dto)
    {
        var response = await PostAsync<CreateSkillResponseDto>("Skills", dto);
        return response?.SkillId ?? 0;
    }
}

public class SkillDto
{
    public int Id { get; set; }
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class CreateSkillDto
{
    public string SkillName { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}

public class CreateSkillResponseDto
{
    public string Message { get; set; } = string.Empty;
    public int SkillId { get; set; }
}
