using LiteQueue.Application.Services;
using LiteQueue.Contracts.Constants.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;


[ApiController]
[Route("api/[controller]")]
public class QueueController : ControllerBase
{
    private readonly QueueService _service;

    public QueueController(QueueService service)
    {
        _service = service;
    }
    [HttpPost("{queueName}")]
    public async Task<IActionResult> CreateQueue(string queueName)
    {
        await _service.CreateQueueAsync(queueName);
        return Ok($"Queue '{queueName}' created.");
    }

    [HttpGet]
    public async Task<IActionResult> ListQueues()
    {
        var queues = await _service.ListQueuesAsync();
        return Ok(queues);
    }

    [HttpPost("{queueName}/messages")]
    public async Task<IActionResult> SendMessage(string queueName, [FromBody] QueueMessageDto message)
    {
        await _service.SendMessageAsync(queueName, message);
        return Ok("Message sent.");
    }

    [HttpGet("{queueName}/messages")]
    public async Task<IActionResult> ReceiveMessages(string queueName, [FromQuery] int max = 1, [FromQuery] int timeout = 30)
    {
        var msgs = await _service.ReceiveMessagesAsync(queueName, max, TimeSpan.FromSeconds(timeout));
        return Ok(msgs);
    }

    [HttpDelete("{queueName}/messages/{messageId}")]
    public async Task<IActionResult> DeleteMessage(string queueName, string messageId)
    {
        await _service.DeleteMessageAsync(queueName, messageId);
        return Ok("Message deleted.");
    }

    [HttpGet("{queueName}/peek")]
    public async Task<IActionResult> PeekMessage(string queueName)
    {
        var msg = await _service.PeekMessageAsync(queueName);
        return Ok(msg);
    }
}