using System.Globalization;
using System.Text.Json;

// No browser: insights print to the console as they happen; the snapshot is written once history
// is loaded and again at the end, so an agent can show the chart early and narrate what follows.
sealed class Headless(Desk desk, IConfiguration config, IHostApplicationLifetime life) : BackgroundService
{
    private static readonly JsonSerializerOptions Json = new(JsonSerializerDefaults.Web) { WriteIndented = true };

    protected override async Task ExecuteAsync(CancellationToken ct)
    {
        double minutes = double.TryParse(config["minutes"], NumberStyles.Float, CultureInfo.InvariantCulture, out double m) && m > 0 ? m : 3;
        string folder = Path.GetFullPath(config["out"] ?? "out");

        try
        {
            await desk.Ready.WaitAsync(TimeSpan.FromMinutes(2), ct);
            Write(folder);
            Console.WriteLine($"Snapshot written to {folder}; streaming live for {minutes:0.#} more minutes.");
            await Task.Delay(TimeSpan.FromMinutes(minutes), ct);
            Write(folder);
            Console.WriteLine($"Done. Final snapshot: {Path.Combine(folder, "desk.html")}");
        }
        catch (TimeoutException)
        {
            Console.WriteLine("No market history arrived within 2 minutes; check network access to the feed.");
            Environment.ExitCode = 1;
        }
        finally
        {
            life.StopApplication();
        }
    }

    private void Write(string folder)
    {
        DeskSnapshot s = desk.Snapshot();
        Directory.CreateDirectory(folder);
        File.WriteAllText(Path.Combine(folder, "desk.html"), Report.Html(s));
        File.WriteAllText(Path.Combine(folder, "desk.svg"), Report.Quadrant(s));
        File.WriteAllText(Path.Combine(folder, "desk.json"), JsonSerializer.Serialize(s, Json));
    }
}
