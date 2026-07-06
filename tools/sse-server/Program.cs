using System.Globalization;
using System.Text.Json;
using FacioQuo.Stock.Indicators;
using Test.Data;
using Test.SseServer;
using TestData = Test.Data.Data;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Minimal-API app: no MVC controllers, so silence the expected MVC
// "No action descriptors found" info log emitted during build-time
// OpenAPI document generation.
builder.Logging.AddFilter(
    "Microsoft.AspNetCore.Mvc.Infrastructure.DefaultActionDescriptorCollectionProvider",
    LogLevel.Warning);

// Add native .NET 10 OpenAPI support
builder.Services.AddOpenApi(options
    => options.AddDocumentTransformer((document, _, _) => {
        document.Info = new() {
            Title = "SSE Bar Server",
            Version = "v1",
            Description = "Server-Sent Events (SSE) endpoint for streaming bar data with configurable delivery rate and time intervals."
        };

        return Task.CompletedTask;
    }));

WebApplication app = builder.Build();

// Configure JSON serialization options
JsonSerializerOptions jsonOptions = new() {
    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true
};

// Streams randomly generated bar data via Server-Sent Events (SSE)
app.MapGet("/bars/random", async (
    HttpContext context,
    int interval = 100,
    int? batchSize = null,
    string barIntervalCode = "1m"
) => {
    // Validate interval parameter
    if (interval <= 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Interval must be greater than 0").ConfigureAwait(false);
        return;
    }

    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";

    int delivered = 0;
    TimeSpan timestampIncrement = ParseInterval(barIntervalCode);

    Console.WriteLine(
        $"[Random] Starting stream - delivery: {interval}ms, barIntervalCode: {barIntervalCode}, batchSize: {batchSize?.ToString(CultureInfo.InvariantCulture) ?? "unlimited"}");

    // Use Test.Data.RandomGbm as a purely incremental generator: start empty
    // (bars: 0) and append one bar per iteration via generator.Add(...) below.
    BarInterval barInterval = barIntervalCode.ToBarInterval();
    RandomGbm generator = new(bars: 0, seed: 1000.0, barInterval: barInterval);
    DateTime currentTimestamp = DateTime.UtcNow.AddMinutes(-1000);

    try
    {
        while (!context.RequestAborted.IsCancellationRequested)
        {
            // Generate next random bar
            generator.Add(currentTimestamp);
            Bar bar = generator[^1];

            // Serialize bar as JSON
            string json = JsonSerializer.Serialize(bar, jsonOptions);

            // Write SSE event manually
            string sseData = $"event: bar\ndata: {json}\n\n";
            await context.Response
                .WriteAsync(sseData, context.RequestAborted)
                .ConfigureAwait(false);
            await context.Response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);

            delivered++;
            currentTimestamp = currentTimestamp.Add(timestampIncrement);

            if (delivered % 100 == 0)
            {
                Console.WriteLine($"[Random] Delivered {delivered} bars");
            }

            // Check if we've reached the batch size limit
            if (batchSize.HasValue && delivered >= batchSize.Value)
            {
                Console.WriteLine($"[Random] Batch complete - delivered {delivered} bars");
                break;
            }

            // Wait for the specified interval
            await Task.Delay(interval, context.RequestAborted).ConfigureAwait(false);
        }
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"[Random] Client disconnected - delivered {delivered} bars");
    }
    catch (IOException)
    {
        Console.WriteLine($"[Random] Connection closed - delivered {delivered} bars");
    }
})
.WithName("GetRandomBars")
.WithTags("Bars")
.AddOpenApiOperationTransformer((operation, _, _) => {
    // Add regex pattern validation for barIntervalCode parameter
    Microsoft.OpenApi.IOpenApiParameter? barIntervalCodeParam = operation.Parameters?.FirstOrDefault(p => p.Name == "barIntervalCode");
    if (barIntervalCodeParam is Microsoft.OpenApi.OpenApiParameter concreteParam && concreteParam.Schema is Microsoft.OpenApi.OpenApiSchema schema)
    {
        schema.Pattern = @"^\d+(\.\d+)?(s|sec|second|seconds|m|min|minute|minutes|h|hr|hour|hours|d|day|days|w|week|weeks|mo|month|months)$";
    }

    return Task.CompletedTask;
});

// Streams historical bar data from the longest available dataset via Server-Sent Events (SSE)
app.MapGet("/bars/longest", async (
    HttpContext context,
    int interval = 100,
    int? batchSize = null,
    string barIntervalCode = "1m",
    string? scenario = null
) => {
    // Validate interval parameter
    if (interval <= 0)
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("Interval must be greater than 0").ConfigureAwait(false);
        return;
    }

    context.Response.ContentType = "text/event-stream";
    context.Response.Headers.CacheControl = "no-cache";

    IReadOnlyList<Bar> longestBars = TestData.GetLongest();

    // Guard against empty data
    if (longestBars == null || longestBars.Count == 0)
    {
        Console.WriteLine("[Longest] ERROR: No bar data available from TestData.GetLongest()");
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync("Server error: No bar data available").ConfigureAwait(false);
        return;
    }

    int totalBars = batchSize ?? longestBars.Count;
    int delivered = 0;
    TimeSpan timestampIncrement = ParseInterval(barIntervalCode);
    DateTime baseTimestamp = longestBars[0].Timestamp;
    List<Bar>? streamedBars = scenario is null ? null : [];

    Console.WriteLine(
        $"[Longest] Starting stream - delivery: {interval}ms, barIntervalCode: {barIntervalCode}, total: {totalBars} bars");

    try
    {
        for (int i = 0; i < totalBars && i < longestBars.Count; i++)
        {
            Bar originalBar = longestBars[i];

            // Create bar with time-warped timestamp
            Bar bar = new(
                Timestamp: baseTimestamp + (timestampIncrement * i),
                Open: originalBar.Open,
                High: originalBar.High,
                Low: originalBar.Low,
                Close: originalBar.Close,
                Volume: originalBar.Volume);

            streamedBars?.Add(bar);

            // Serialize bar as JSON
            string json = JsonSerializer.Serialize(bar, jsonOptions);

            // Write SSE event manually
            string sseData = $"event: bar\ndata: {json}\n\n";
            await context.Response
                .WriteAsync(sseData, context.RequestAborted)
                .ConfigureAwait(false);
            await context.Response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);

            delivered++;

            if (delivered % 100 == 0)
            {
                Console.WriteLine($"[Longest] Delivered {delivered}/{totalBars} bars");
            }

            // Wait for the specified interval
            await Task.Delay(interval, context.RequestAborted).ConfigureAwait(false);
        }

        if (streamedBars is not null)
        {
            await SendScenarioEvents(context, scenario, streamedBars, interval, jsonOptions).ConfigureAwait(false);
        }

        Console.WriteLine($"[Longest] Stream complete - delivered {delivered} bars");
    }
    catch (OperationCanceledException)
    {
        Console.WriteLine($"[Longest] Client disconnected - delivered {delivered}/{totalBars} bars");
    }
    catch (IOException)
    {
        Console.WriteLine($"[Longest] Connection closed - delivered {delivered}/{totalBars} bars");
    }
})
.WithName("GetLongestBars")
.WithTags("Bars")
.AddOpenApiOperationTransformer((operation, _, _) => {
    // Add regex pattern validation for barIntervalCode parameter
    Microsoft.OpenApi.IOpenApiParameter? barIntervalCodeParam = operation.Parameters?.FirstOrDefault(p => p.Name == "barIntervalCode");
    if (barIntervalCodeParam is Microsoft.OpenApi.OpenApiParameter concreteParam && concreteParam.Schema is Microsoft.OpenApi.OpenApiSchema schema)
    {
        schema.Pattern = @"^\d+(\.\d+)?(s|sec|second|seconds|m|min|minute|minutes|h|hr|hour|hours|d|day|days|w|week|weeks|mo|month|months)$";
    }

    return Task.CompletedTask;
});

Console.WriteLine("SSE Server starting...");
Console.WriteLine("Endpoints:");
Console.WriteLine("  - /bars/random?interval=100&batchSize=1000&barIntervalCode=1h");
Console.WriteLine("  - /bars/longest?interval=100&batchSize=1000&barIntervalCode=5m");
Console.WriteLine("  - /openapi/v1.json for OpenAPI JSON specification");

// Map OpenAPI endpoint (dev environment only for security)
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

await app.RunAsync().ConfigureAwait(false);

#pragma warning disable CS1587, RCS1263 // XML comment warnings for local function
/// <summary>
/// Parse interval string (e.g., "1h", "5m", "30s") to TimeSpan
/// </summary>
/// <param name="intervalString">Time interval string</param>
/// <returns>Parsed TimeSpan or 1 minute if parsing fails</returns>
static TimeSpan ParseInterval(string intervalString)
#pragma warning restore CS1587, RCS1263
{
    if (string.IsNullOrWhiteSpace(intervalString))
    {
        return TimeSpan.FromMinutes(1);
    }

    string trimmed = intervalString.Trim().ToLowerInvariant();

    // Extract numeric part and unit
    int i = 0;
    while (i < trimmed.Length && (char.IsDigit(trimmed[i]) || trimmed[i] == '.'))
    {
        i++;
    }

    if (i == 0 || !double.TryParse(trimmed[..i], out double value))
    {
        return TimeSpan.FromMinutes(1);
    }

    string unit = trimmed[i..].Trim();

    return unit switch {
        "s" or "sec" or "second" or "seconds" => TimeSpan.FromSeconds(value),
        "m" or "min" or "minute" or "minutes" => TimeSpan.FromMinutes(value),
        "h" or "hr" or "hour" or "hours" => TimeSpan.FromHours(value),
        "d" or "day" or "days" => TimeSpan.FromDays(value),
        _ => TimeSpan.FromMinutes(1)
    };
}

static async Task SendScenarioEvents(
    HttpContext context,
    string? scenario,
    IReadOnlyList<Bar> streamedBars,
    int interval,
    JsonSerializerOptions jsonOptions)
{
    if (string.IsNullOrWhiteSpace(scenario))
    {
        return;
    }

    List<SseBarAction> actions = scenario switch {
        "stc-rollbacks" => BuildStcRollbackActions(streamedBars),
        "allhubs-rollbacks" => BuildAllHubsRollbackActions(streamedBars),
        _ => []
    };

    if (actions.Count == 0)
    {
        return;
    }

    foreach (SseBarAction action in actions)
    {
        string json = JsonSerializer.Serialize(action.Payload, jsonOptions);
        string sseData = $"event: {action.EventType}\ndata: {json}\n\n";
        await context.Response
            .WriteAsync(sseData, context.RequestAborted)
            .ConfigureAwait(false);
        await context.Response.Body.FlushAsync(context.RequestAborted).ConfigureAwait(false);
        await Task.Delay(interval, context.RequestAborted).ConfigureAwait(false);
    }
}

static List<SseBarAction> BuildStcRollbackActions(IReadOnlyList<Bar> streamedBars)
{
    List<SseBarAction> actions = [];

    // After streaming 2000 bars with MaxCacheSize=1500, cache contains bars 500-1999.
    // Use same-timestamp revisions (Add) for the last bar and also test Remove operations.

    if (streamedBars.Count > 0)
    {
        Bar lastBar = streamedBars[^1];

        // Add a Remove action for a bar inside the cached window (not the last bar)
        // This exercises the remove/rollback and rebuild-on-remove paths
        // Cache contains bars 500-1999, so removing bar at index 1900 (cache index ~1400)
        if (streamedBars.Count > 1900)
        {
            const int removeIndex = 1900; // Bar within cache range
            const int cacheIndex = removeIndex - 500; // Approximate cache index (500 is the first cached bar)
            actions.Add(SseBarAction.Remove(cacheIndex, streamedBars[removeIndex]));
        }

        // Perform multiple revisions on the last bar to test rollback functionality
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close * 0.99m,
            lastBar.Volume)));
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close * 1.01m,
            lastBar.Volume)));
        // Restore original values (create new instance to avoid duplicate detection)
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close,
            lastBar.Volume)));
    }

    return actions;
}

static List<SseBarAction> BuildAllHubsRollbackActions(IReadOnlyList<Bar> streamedBars)
{
    List<SseBarAction> actions = [];

    // After streaming 2000 bars with MaxCacheSize=1500, cache contains bars 500-1999.
    // Use same-timestamp revisions (Add) for the last bar and also test Remove operations.

    if (streamedBars.Count > 0)
    {
        Bar lastBar = streamedBars[^1];

        // Add a Remove action for a bar inside the cached window (not the last bar)
        // This exercises the remove/rollback and rebuild-on-remove paths
        // Cache contains bars 500-1999, so removing bar at index 1800 (cache index ~1300)
        if (streamedBars.Count > 1800)
        {
            const int removeIndex = 1800; // Bar within cache range
            const int cacheIndex = removeIndex - 500; // Approximate cache index (500 is the first cached bar)
            actions.Add(SseBarAction.Remove(cacheIndex, streamedBars[removeIndex]));
        }

        // Perform multiple revisions on the last bar to test rollback functionality
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close * 0.99m,
            lastBar.Volume)));
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close * 1.01m,
            lastBar.Volume)));
        // Restore original values (create new instance to avoid duplicate detection)
        actions.Add(SseBarAction.Add(new Bar(
            lastBar.Timestamp,
            lastBar.Open,
            lastBar.High,
            lastBar.Low,
            lastBar.Close,
            lastBar.Volume)));
    }

    return actions;
}
