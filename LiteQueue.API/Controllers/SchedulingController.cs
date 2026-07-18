using LiteQueue.Application.Services;
using LiteQueue.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;

[ApiController]
[Route("api/scheduling")]
public class SchedulingController : ControllerBase
{
    private readonly SchedulingService _service;

    public SchedulingController(SchedulingService service)
    {
        _service = service;
    }

    [HttpPost("schedules")]
    public async Task<IActionResult> ScheduleMessage([FromBody] ScheduleMessageRequest request)
    {
        await _service.ScheduleMessageAsync(request.QueueName, request.Body, request.ExecuteAt);
        return Ok(new { status = "scheduled" });
    }

    [HttpGet("schedules/pending")]
    public async Task<IActionResult> GetPendingSchedules()
    {
        var pending = await _service.GetPendingSchedulesAsync();
        return Ok(pending);
    }

    [HttpPost("recurring")]
    public async Task<IActionResult> CreateRecurringJob([FromBody] CreateRecurringJobRequest request)
    {
        var job = new RecurringJob
        {
            Id = request.JobId,
            QueueName = request.QueueName,
            CronExpression = request.CronExpression,
            TimeZone = request.TimeZone ?? "UTC",
            Body = request.Body,
            Enabled = true
        };
        await _service.CreateRecurringJobAsync(job);
        return Ok(new { jobId = job.Id, status = "created" });
    }

    [HttpGet("recurring")]
    public async Task<IActionResult> GetAllRecurringJobs()
    {
        var jobs = await _service.GetAllRecurringJobsAsync();
        return Ok(jobs);
    }

    [HttpGet("recurring/{jobId}")]
    public async Task<IActionResult> GetRecurringJob([FromRoute] string jobId)
    {
        var job = await _service.GetRecurringJobAsync(jobId);
        if (job == null)
            return NotFound(new { error = "Recurring job not found" });

        return Ok(job);
    }

    [HttpPut("recurring/{jobId}")]
    public async Task<IActionResult> UpdateRecurringJob(
        [FromRoute] string jobId,
        [FromBody] UpdateRecurringJobRequest request)
    {
        var job = await _service.GetRecurringJobAsync(jobId);
        if (job == null)
            return NotFound(new { error = "Recurring job not found" });

        if (!string.IsNullOrWhiteSpace(request.CronExpression))
            job.CronExpression = request.CronExpression;
        if (!string.IsNullOrWhiteSpace(request.TimeZone))
            job.TimeZone = request.TimeZone;
        if (!string.IsNullOrWhiteSpace(request.Body))
            job.Body = request.Body;
        if (!string.IsNullOrWhiteSpace(request.QueueName))
            job.QueueName = request.QueueName;

        await _service.UpdateRecurringJobAsync(job);
        return Ok(job);
    }

    [HttpDelete("recurring/{jobId}")]
    public async Task<IActionResult> DeleteRecurringJob([FromRoute] string jobId)
    {
        await _service.DeleteRecurringJobAsync(jobId);
        return Ok(new { jobId, status = "deleted" });
    }

    [HttpPost("recurring/{jobId}/trigger")]
    public async Task<IActionResult> TriggerRecurringJob([FromRoute] string jobId)
    {
        var job = await _service.GetRecurringJobAsync(jobId);
        if (job == null)
            return NotFound(new { error = "Recurring job not found" });

        var now = DateTimeOffset.UtcNow;
        await _service.ScheduleMessageAsync(job.QueueName, job.Body, now);
        return Ok(new { jobId, status = "triggered", triggeredAt = now });
    }
}

public class ScheduleMessageRequest
{
    public string QueueName { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public DateTimeOffset ExecuteAt { get; set; }
}

public class CreateRecurringJobRequest
{
    public string JobId { get; set; } = string.Empty;
    public string QueueName { get; set; } = string.Empty;
    public string CronExpression { get; set; } = string.Empty;
    public string? TimeZone { get; set; }
    public string Body { get; set; } = string.Empty;
}

public class UpdateRecurringJobRequest
{
    public string? QueueName { get; set; }
    public string? CronExpression { get; set; }
    public string? TimeZone { get; set; }
    public string? Body { get; set; }
}
