using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

// LiteQueue.NET — AI Document Processing Pipeline Demo
// Demonstrates: enqueue, pub/sub fan-out, delayed retries, DLQ, scheduled jobs, recurring jobs

var client = new HttpClient { BaseAddress = new Uri("http://localhost:5135") };
var jsonOptions = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

async Task<string> EnqueueAsync(string queueName, object message)
{
    var content = new StringContent(JsonSerializer.Serialize(message, jsonOptions), Encoding.UTF8, "application/json");
    var response = await client.PostAsync($"/api/queues/{queueName}/messages", content);
    response.EnsureSuccessStatusCode();
    var result = await response.Content.ReadFromJsonAsync<JsonElement>();
    return result.GetProperty("messageId").GetString()!;
}

async Task PublishAsync(string topicName, object message)
{
    var content = new StringContent(JsonSerializer.Serialize(message, jsonOptions), Encoding.UTF8, "application/json");
    var response = await client.PostAsync($"/api/topics/{topicName}/messages", content);
    response.EnsureSuccessStatusCode();
    Console.WriteLine($"  Published to topic '{topicName}'");
}

Console.WriteLine("=== LiteQueue.NET AI Document Processing Pipeline Demo ===\n");

// Step 1: Create a topic for document uploads
Console.WriteLine("[1] Creating 'document-uploaded' topic...");
try { await client.PostAsync("/api/topics/document-uploaded", null); } catch { }

// Step 2: Create subscriptions for virus scan, text extraction, and audit
Console.WriteLine("[2] Setting up subscriptions...");
await client.PostAsync("/api/topics/document-uploaded/subscriptions",
    new StringContent(JsonSerializer.Serialize(new { subscriptionName = "virus-scan" }, jsonOptions), Encoding.UTF8, "application/json"));
await client.PostAsync("/api/topics/document-uploaded/subscriptions",
    new StringContent(JsonSerializer.Serialize(new { subscriptionName = "text-extraction" }, jsonOptions), Encoding.UTF8, "application/json"));
await client.PostAsync("/api/topics/document-uploaded/subscriptions",
    new StringContent(JsonSerializer.Serialize(new { subscriptionName = "audit" }, jsonOptions), Encoding.UTF8, "application/json"));

// Step 3: Publish a document upload event (simulates fan-out to 3 subscribers)
Console.WriteLine("[3] Publishing document upload event...");
await PublishAsync("document-uploaded", new { documentId = "doc-001", fileName = "report.pdf", userId = "user-42" });

// Step 4: Enqueue work to AI analysis queue
Console.WriteLine("[4] Enqueueing AI analysis job...");
var aiMessageId = await EnqueueAsync("ai-analysis", new { documentId = "doc-001", model = "gpt-4" });

// Step 5: Enqueue work to report generation queue
Console.WriteLine("[5] Enqueueing report generation job...");
var reportMessageId = await EnqueueAsync("report-gen", new { documentId = "doc-001", type = "summary" });

// Step 6: Enqueue notification with delay (reminder in 30 min)
Console.WriteLine("[6] Enqueueing delayed notification...");
var notifyContent = new StringContent(JsonSerializer.Serialize(new {
    body = "{\"userId\":\"user-42\",\"message\":\"Your report is ready\"}",
    delaySeconds = 1800
}, jsonOptions), Encoding.UTF8, "application/json");
await client.PostAsync("/api/queues/notifications/messages", notifyContent);

// Step 7: Schedule a recurring cleanup job
Console.WriteLine("[7] Creating recurring cleanup job...");
var cleanupContent = new StringContent(JsonSerializer.Serialize(new {
    jobId = "daily-document-cleanup",
    queueName = "cleanup",
    cronExpression = "0 0 3 * * *",
    timeZone = "UTC",
    body = "{\"action\":\"purge-expired\"}"
}, jsonOptions), Encoding.UTF8, "application/json");
await client.PostAsync("/api/scheduling/recurring", cleanupContent);

// Step 8: Show queue statistics
Console.WriteLine("\n[8] Queue Statistics:");
var statsResponse = await client.GetAsync("/api/queues/ai-analysis/statistics");
var stats = await statsResponse.Content.ReadFromJsonAsync<JsonElement>();
Console.WriteLine($"  ai-analysis: ready={stats.GetProperty("readyCount").GetInt64()}, inflight={stats.GetProperty("inFlightCount").GetInt64()}");

// Step 9: Show topic subscriptions
Console.WriteLine("\n[9] Topic Subscriptions for 'document-uploaded':");
var subsResponse = await client.GetAsync("/api/topics/document-uploaded/subscriptions");
var subs = await subsResponse.Content.ReadFromJsonAsync<JsonElement[]>();
foreach (var sub in subs!)
    Console.WriteLine($"  - {sub.GetString()}");

// Step 10: Recurring jobs
Console.WriteLine("\n[10] Recurring Jobs:");
var recurringResponse = await client.GetAsync("/api/scheduling/recurring");
if (recurringResponse.IsSuccessStatusCode)
{
    var jobs = await recurringResponse.Content.ReadFromJsonAsync<JsonElement[]>();
    foreach (var job in jobs!)
        Console.WriteLine($"  - {job.GetProperty("id").GetString()}: {job.GetProperty("cronExpression").GetString()} (enabled={job.GetProperty("enabled").GetBoolean()})");
}

Console.WriteLine("\n=== Demo Complete ===");
Console.WriteLine($"Message IDs: AI Analysis={aiMessageId}, Report={reportMessageId}");
