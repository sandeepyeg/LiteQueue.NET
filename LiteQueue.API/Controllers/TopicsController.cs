using LiteQueue.Application.Services;
using LiteQueue.Contracts.Topics;
using LiteQueue.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace LiteQueue.API.Controllers;

[ApiController]
[Route("api/topics")]
public class TopicsController : ControllerBase
{
    private readonly TopicService _service;

    public TopicsController(TopicService service)
    {
        _service = service;
    }

    [HttpPost("{topicName}")]
    public async Task<IActionResult> CreateTopic([FromRoute] string topicName)
    {
        await _service.CreateTopicAsync(topicName);
        return Ok(new { topic = topicName, status = "created" });
    }

    [HttpDelete("{topicName}")]
    public async Task<IActionResult> DeleteTopic([FromRoute] string topicName)
    {
        await _service.DeleteTopicAsync(topicName);
        return Ok(new { topic = topicName, status = "deleted" });
    }

    [HttpGet]
    public async Task<IActionResult> ListTopics()
    {
        var topics = await _service.ListTopicsAsync();
        return Ok(topics);
    }

    [HttpGet("{topicName}")]
    public async Task<IActionResult> GetTopic([FromRoute] string topicName)
    {
        var topic = await _service.GetTopicAsync(topicName);
        if (topic == null)
            return NotFound(new { error = "Topic not found" });

        return Ok(topic);
    }

    [HttpPost("{topicName}/subscriptions")]
    public async Task<IActionResult> CreateSubscription(
        [FromRoute] string topicName,
        [FromBody] CreateSubscriptionRequest request)
    {
        await _service.CreateSubscriptionAsync(topicName, request.SubscriptionName);
        return Ok(new { topic = topicName, subscription = request.SubscriptionName, status = "created" });
    }

    [HttpDelete("{topicName}/subscriptions/{subscriptionName}")]
    public async Task<IActionResult> DeleteSubscription(
        [FromRoute] string topicName,
        [FromRoute] string subscriptionName)
    {
        await _service.DeleteSubscriptionAsync(topicName, subscriptionName);
        return Ok(new { topic = topicName, subscription = subscriptionName, status = "deleted" });
    }

    [HttpGet("{topicName}/subscriptions")]
    public async Task<IActionResult> ListSubscriptions([FromRoute] string topicName)
    {
        var subscriptions = await _service.ListSubscriptionsAsync(topicName);
        return Ok(subscriptions);
    }

    [HttpPost("{topicName}/messages")]
    public async Task<IActionResult> PublishMessage(
        [FromRoute] string topicName,
        [FromBody] PublishMessageRequest request)
    {
        var message = new QueueMessage
        {
            Id = Guid.NewGuid().ToString(),
            Body = request.Body,
            CreatedAt = DateTimeOffset.UtcNow
        };
        await _service.PublishAsync(topicName, message);
        return Ok(new { messageId = message.Id });
    }
}
