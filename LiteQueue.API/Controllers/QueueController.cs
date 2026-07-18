using LiteQueue.Application.Services;
using LiteQueue.Contracts.Messages;
using LiteQueue.Contracts.Queues;
using LiteQueue.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;

[ApiController]
[Route("api/queues")]
public class QueueController : ControllerBase
{
    private readonly QueueService _service;
    private readonly MessageDeduplicationService _deduplication;

    public QueueController(QueueService service, MessageDeduplicationService deduplication)
    {
        _service = service;
        _deduplication = deduplication;
    }

    [HttpPost("{queueName}")]
    public async Task<IActionResult> CreateQueue([FromRoute] string queueName)
    {
        await _service.CreateQueueAsync(queueName);
        return Ok(new { queue = queueName, status = "created" });
    }

    [HttpGet]
    public async Task<IActionResult> ListQueues()
    {
        var queues = await _service.ListQueuesAsync();
        return Ok(queues);
    }

    [HttpPost("{queueName}/messages")]
    public async Task<IActionResult> SendMessage([FromRoute] string queueName, [FromBody] EnqueueMessageRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
        {
            var existingId = await _deduplication.TryDeduplicateAsync(queueName, request.IdempotencyKey);
            if (existingId != null)
                return Ok(new { messageId = existingId, deduplicated = true });
        }

        var message = new QueueMessage
        {
            Id = Guid.NewGuid().ToString(),
            Body = request.Body,
            CreatedAt = DateTimeOffset.UtcNow
        };

        if (request.DelaySeconds > 0)
        {
            await _service.SendMessageWithDelayAsync(queueName, message, TimeSpan.FromSeconds(request.DelaySeconds.Value));
        }
        else if (request.AvailableAt.HasValue)
        {
            var delay = request.AvailableAt.Value - DateTimeOffset.UtcNow;
            await _service.SendMessageWithDelayAsync(queueName, message, delay > TimeSpan.Zero ? delay : TimeSpan.Zero);
        }
        else
        {
            await _service.SendMessageAsync(queueName, message);
        }

        if (!string.IsNullOrWhiteSpace(request.IdempotencyKey))
            await _deduplication.StoreIdempotencyKeyAsync(queueName, request.IdempotencyKey, message.Id);

        return Ok(new { messageId = message.Id });
    }

    [HttpPost("{queueName}/receive")]
    public async Task<IActionResult> ReceiveMessages(
        [FromRoute] string queueName,
        [FromQuery] int max = 1,
        [FromQuery] int timeout = 30)
    {
        var messages = await _service.ReceiveMessagesAsync(queueName, max, TimeSpan.FromSeconds(timeout));
        return Ok(messages);
    }

    [HttpPost("{queueName}/receive/longpoll")]
    public async Task<IActionResult> ReceiveWithLongPolling(
        [FromRoute] string queueName,
        [FromQuery] int max = 1,
        [FromQuery] int visibilityTimeout = 30,
        [FromQuery] int longPollTimeout = 30)
    {
        var messages = await _service.ReceiveWithLongPollingAsync(
            queueName, max, TimeSpan.FromSeconds(visibilityTimeout), TimeSpan.FromSeconds(longPollTimeout));
        return Ok(messages);
    }

    [HttpPost("{queueName}/acknowledge")]
    public async Task<IActionResult> AcknowledgeMessage(
        [FromRoute] string queueName,
        [FromBody] AcknowledgeRequest request)
    {
        await _service.AcknowledgeMessageAsync(queueName, request.ReceiptHandle);
        return Ok();
    }

    [HttpPost("{queueName}/reject")]
    public async Task<IActionResult> RejectMessage(
        [FromRoute] string queueName,
        [FromBody] RejectRequest request)
    {
        await _service.RejectMessageAsync(queueName, request.ReceiptHandle);
        return Ok();
    }

    [HttpGet("{queueName}/peek")]
    public async Task<IActionResult> PeekMessage([FromRoute] string queueName)
    {
        var message = await _service.PeekMessageAsync(queueName);
        return Ok(message);
    }

    [HttpGet("{queueName}/deadletters")]
    public async Task<IActionResult> GetDeadLetters([FromRoute] string queueName)
    {
        var messages = await _service.GetDeadLetterMessagesAsync(queueName);
        return Ok(messages);
    }

    [HttpPost("{queueName}/deadletters/{messageId}/redrive")]
    public async Task<IActionResult> RedriveDeadLetter(
        [FromRoute] string queueName,
        [FromRoute] string messageId)
    {
        await _service.RedriveDeadLetterMessageAsync(queueName, messageId);
        return Ok();
    }

    [HttpPatch("{queueName}")]
    public async Task<IActionResult> UpdateQueueConfiguration(
        [FromRoute] string queueName,
        [FromBody] UpdateQueueConfigRequest request)
    {
        var existing = await _service.GetQueueConfigurationAsync(queueName);
        if (existing == null)
            return NotFound(new { error = "Queue not found" });

        if (request.MaxDeliveryAttempts.HasValue)
            existing.MaxDeliveryAttempts = request.MaxDeliveryAttempts.Value;
        if (request.VisibilityTimeoutSeconds.HasValue)
            existing.VisibilityTimeoutSeconds = request.VisibilityTimeoutSeconds.Value;
        if (request.MessageRetentionSeconds.HasValue)
            existing.MessageRetentionSeconds = request.MessageRetentionSeconds.Value;
        if (request.DeadLetterRetentionSeconds.HasValue)
            existing.DeadLetterRetentionSeconds = request.DeadLetterRetentionSeconds.Value;
        if (request.MaxMessageSizeBytes.HasValue)
            existing.MaxMessageSizeBytes = request.MaxMessageSizeBytes.Value;

        await _service.UpdateQueueConfigurationAsync(queueName, existing);
        return Ok(existing);
    }

    [HttpGet("{queueName}")]
    public async Task<IActionResult> GetQueue([FromRoute] string queueName)
    {
        var config = await _service.GetQueueConfigurationAsync(queueName);
        if (config == null)
            return NotFound(new { error = "Queue not found" });

        return Ok(config);
    }

    [HttpDelete("{queueName}")]
    public async Task<IActionResult> DeleteQueue([FromRoute] string queueName)
    {
        await _service.PurgeQueueAsync(queueName);
        return Ok(new { queue = queueName, status = "deleted" });
    }

    [HttpPost("{queueName}/pause")]
    public async Task<IActionResult> PauseQueue([FromRoute] string queueName)
    {
        await _service.PauseQueueAsync(queueName);
        return Ok(new { queue = queueName, status = "paused" });
    }

    [HttpPost("{queueName}/resume")]
    public async Task<IActionResult> ResumeQueue([FromRoute] string queueName)
    {
        await _service.ResumeQueueAsync(queueName);
        return Ok(new { queue = queueName, status = "resumed" });
    }

    [HttpPost("{queueName}/purge")]
    public async Task<IActionResult> PurgeQueue([FromRoute] string queueName)
    {
        await _service.PurgeQueueAsync(queueName);
        return Ok(new { queue = queueName, status = "purged" });
    }

    [HttpGet("{queueName}/statistics")]
    public async Task<IActionResult> GetQueueStatistics([FromRoute] string queueName)
    {
        var stats = await _service.GetQueueStatisticsAsync(queueName);
        var config = await _service.GetQueueConfigurationAsync(queueName);

        return Ok(new QueueSummaryResponse
        {
            Name = queueName,
            Status = config?.Status ?? "Active",
            ReadyCount = stats.ReadyCount,
            InFlightCount = stats.InFlightCount,
            DelayedCount = stats.DelayedCount,
            DeadLetterCount = stats.DeadLetterCount,
            CreatedAt = config?.CreatedAt ?? DateTimeOffset.UtcNow
        });
    }

    [HttpPost("{queueName}/visibility")]
    public async Task<IActionResult> ExtendVisibility(
        [FromRoute] string queueName,
        [FromBody] ChangeVisibilityRequest request)
    {
        await _service.ExtendVisibilityAsync(queueName, request.ReceiptHandle, TimeSpan.FromSeconds(request.VisibilityTimeoutSeconds));
        return Ok();
    }

    [HttpPost("{queueName}/messages/batch")]
    public async Task<IActionResult> BatchEnqueue(
        [FromRoute] string queueName,
        [FromBody] List<EnqueueMessageRequest> requests)
    {
        var messageIds = new List<string>();
        foreach (var req in requests)
        {
            var message = new QueueMessage
            {
                Id = Guid.NewGuid().ToString(),
                Body = req.Body,
                CreatedAt = DateTimeOffset.UtcNow
            };
            await _service.SendMessageAsync(queueName, message);
            messageIds.Add(message.Id);
        }
        return Ok(new { messageIds });
    }

    [HttpPost("{queueName}/deadletters/redrive")]
    public async Task<IActionResult> BatchRedriveDeadLetters([FromRoute] string queueName)
    {
        var deadLetters = await _service.GetDeadLetterMessagesAsync(queueName);
        foreach (var dl in deadLetters)
        {
            await _service.RedriveDeadLetterMessageAsync(queueName, dl.Id);
        }
        return Ok(new { redriveCount = deadLetters.Count() });
    }

    [HttpDelete("{queueName}/deadletters/{messageId}")]
    public async Task<IActionResult> DeleteDeadLetterMessage(
        [FromRoute] string queueName,
        [FromRoute] string messageId)
    {
        var deleted = await _service.DeleteDeadLetterMessageAsync(queueName, messageId);
        if (!deleted)
            return NotFound(new { error = "Dead letter message not found" });

        return Ok(new { messageId, status = "deleted" });
    }

    [HttpGet("{queueName}/deadletters/{messageId}")]
    public async Task<IActionResult> GetDeadLetterMessage(
        [FromRoute] string queueName,
        [FromRoute] string messageId)
    {
        var deadLetters = await _service.GetDeadLetterMessagesAsync(queueName);
        var message = deadLetters.FirstOrDefault(d => d.Id == messageId || d.OriginalMessageId == messageId);
        if (message == null)
            return NotFound(new { error = "Dead letter message not found" });

        return Ok(message);
    }

    [HttpGet("{queueName}/messages")]
    public async Task<IActionResult> GetMessages(
        [FromRoute] string queueName,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var stats = await _service.GetQueueStatisticsAsync(queueName);
        return Ok(new { queueName, stats.ReadyCount, stats.InFlightCount });
    }

    [HttpGet("{queueName}/inflight")]
    public async Task<IActionResult> GetInFlightMessages([FromRoute] string queueName)
    {
        var stats = await _service.GetQueueStatisticsAsync(queueName);
        return Ok(new { queueName, inFlightCount = stats.InFlightCount });
    }
}

public record UpdateQueueConfigRequest(
    int? MaxDeliveryAttempts,
    int? VisibilityTimeoutSeconds,
    int? MessageRetentionSeconds,
    int? DeadLetterRetentionSeconds,
    int? MaxMessageSizeBytes);
