using LiteQueue.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;

[ApiController]
[Route("api/admin")]
public class AdminController : ControllerBase
{
    private readonly QueueMonitoringService _monitoringService;
    private readonly QueueService _queueService;

    public AdminController(QueueMonitoringService monitoringService, QueueService queueService)
    {
        _monitoringService = monitoringService;
        _queueService = queueService;
    }

    [HttpGet("statistics")]
    public async Task<IActionResult> GetStatistics()
    {
        var stats = await _monitoringService.GetAllQueuesStatisticsAsync();
        return Ok(stats);
    }

    [HttpGet("health")]
    public IActionResult GetHealth()
    {
        return Ok(new
        {
            status = "healthy",
            timestamp = DateTimeOffset.UtcNow,
            version = "1.0.0"
        });
    }
}
