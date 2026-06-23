using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PRM.Core.Entities;
using PRM.Infrastructure.Data;
using System.Threading.Tasks;

namespace PRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "System.Configure")]
public class SystemConfigController : ControllerBase
{
    private readonly PrmDbContext _context;

    public SystemConfigController(PrmDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllConfigs()
    {
        var configs = await _context.SystemConfigs.ToListAsync();
        return Ok(configs);
    }

    [HttpGet("{key}")]
    public async Task<IActionResult> GetConfig(string key)
    {
        var config = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == key);
        if (config == null) return NotFound();
        return Ok(new { config.Key, config.Value });
    }

    [HttpPost]
    public async Task<IActionResult> UpdateConfig([FromBody] SystemConfig configUpdate)
    {
        if (string.IsNullOrWhiteSpace(configUpdate.Key)) return BadRequest("Key is required.");

        var existing = await _context.SystemConfigs.FirstOrDefaultAsync(c => c.Key == configUpdate.Key);
        if (existing != null)
        {
            existing.Value = configUpdate.Value;
        }
        else
        {
            _context.SystemConfigs.Add(new SystemConfig { Key = configUpdate.Key, Value = configUpdate.Value });
        }

        await _context.SaveChangesAsync();
        return Ok(new { configUpdate.Key, configUpdate.Value });
    }
}
