using Alpaca.Markets;

namespace Tests.Data;

internal static class FeedData
{
    internal static async Task<IEnumerable<Bar>> GetBars(string symbol)
        => await GetBars(symbol, 365 * 2)
                .ConfigureAwait(false);

    internal static async Task<IEnumerable<Bar>> GetBars(string symbol, int days)
    {
        /* This won't run if environment variables not set.
           Use FeedData.InconclusiveIfNotSetup() in tests.

           (1) get your API keys
           https://alpaca.markets/docs/market-data/getting-started/

           (2) manually install in your environment (replace value)

           setx ALPACA_KEY "y0ur_Alp@ca_K3Y_v@lue"
           setx ALPACA_SECRET "y0ur_Alp@ca_S3cret_v@lue"

         ****************************************************/

        // get and validate keys
        string? alpacaApiKey = Environment.GetEnvironmentVariable("ALPACA_KEY");
        string? alpacaSecret = Environment.GetEnvironmentVariable("ALPACA_SECRET");

        if (string.IsNullOrEmpty(alpacaApiKey) || string.IsNullOrEmpty(alpacaSecret))
        {
            Assert.Inconclusive("Data feed unusable. Environment variables missing.");
        }

        ArgumentException.ThrowIfNullOrEmpty(alpacaApiKey, nameof(alpacaApiKey));
        ArgumentException.ThrowIfNullOrEmpty(alpacaSecret, nameof(alpacaSecret));

        // connect to Alpaca REST API
        SecretKey secretKey = new(alpacaApiKey, alpacaSecret);

        IAlpacaDataClient client = Environments
            .Paper
            .GetAlpacaDataClient(secretKey);

        // compose request
        // (excludes last 16 minutes for free delayed bars)
        DateTime into = DateTime.UtcNow.Subtract(TimeSpan.FromMinutes(16));
        DateTime from = into.Subtract(TimeSpan.FromDays(days));

        HistoricalBarsRequest request = new(symbol, from, into, BarTimeFrame.Day);

        // fetch daily bars in Alpaca's format
        IPage<Alpaca.Markets.IBar> barSet = await client
            .ListHistoricalBarsAsync(request)
            .ConfigureAwait(false);

        // convert library compatible bars
        return barSet
            .Items
            .Select(static bar => new Bar
             (
                Timestamp: bar.TimeUtc,
                Open: bar.Open,
                High: bar.High,
                Low: bar.Low,
                Close: bar.Close,
                Volume: bar.Volume
             ))
            .OrderBy(static x => x.Timestamp);
    }
}
