using LiteQueue.Application.Services;
using LiteQueue.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;

[ApiController]
[Route("api/queues")]
public class QueueController : ControllerBase
{
    private readonly QueueService _service;

    public QueueController(QueueService service)
    {
        _service = service;
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
    public async Task<IActionResult> SendMessage([FromRoute] string queueName, [FromBody] QueueMessage message)
    {
        message.Id = Guid.NewGuid().ToString();
        message.CreatedAt = DateTimeOffset.UtcNow;
        await _service.SendMessageAsync(queueName, message);
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
}

public record AcknowledgeRequest(string ReceiptHandle);
public record RejectRequest(string ReceiptHandle);
