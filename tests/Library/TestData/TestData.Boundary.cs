namespace Test.Data;

/// <summary>
/// Generates boundary test data designed to expose floating-point precision issues
/// in bounded indicators. These datasets push calculations to exact mathematical
/// boundaries where precision errors become visible.
/// </summary>
internal static class BoundaryBars
{
    /// <summary>
    /// Generates bars with monotonically increasing closes to push RSI toward 100.
    /// After warmup, all gains with no losses should theoretically produce RSI = 100.
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <param name="startPrice">Starting price value.</param>
    /// <param name="increment">Price increment per bar.</param>
    /// <returns>List of bars with strictly increasing closes.</returns>
    internal static IReadOnlyList<Bar> GetMonotonicallyIncreasing(
        int bars = 100,
        decimal startPrice = 100m,
        decimal increment = 0.01m)
    {
        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            decimal close = startPrice + (i * increment);
            // Valid OHLC: High is max, Low is min, Open and Close between them
            decimal open = close - increment;
            decimal high = close + increment;
            decimal low = open - increment;  // Low is minimum of all values
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: open,
                High: high,
                Low: low,
                Close: close,
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates bars with monotonically decreasing closes to push RSI toward 0.
    /// After warmup, all losses with no gains should theoretically produce RSI = 0.
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <param name="startPrice">Starting price value.</param>
    /// <param name="decrement">Price decrement per bar.</param>
    /// <returns>List of bars with strictly decreasing closes.</returns>
    internal static IReadOnlyList<Bar> GetMonotonicallyDecreasing(
        int bars = 100,
        decimal startPrice = 1000m,
        decimal decrement = 0.01m)
    {
        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            decimal close = startPrice - (i * decrement);
            // Valid bearish OHLC: High is max (above open), Low is min (below close)
            decimal open = close + decrement;
            decimal high = open + decrement;  // High is maximum of all values
            decimal low = close - decrement;  // Low is minimum of all values
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: open,
                High: high,
                Low: low,
                Close: close,
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates bars where Close = High for Stoch boundary testing.
    /// When Close equals the highest high in the lookback, Stoch should be exactly 100.
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <returns>List of bars where Close equals High.</returns>
    internal static IReadOnlyList<Bar> GetCloseEqualsHigh(int bars = 100)
    {
        // Range offsets for creating realistic candles where Close = High
        const decimal openOffset = 1m;  // Open below Close/High
        const decimal lowOffset = 2m;   // Low below Open

        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            decimal price = 100m + (i * 0.1m);
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: price - openOffset,
                High: price,                    // Close = High (boundary condition)
                Low: price - lowOffset,
                Close: price,                   // Close = High (boundary condition)
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates bars where Close = Low for Stoch boundary testing.
    /// When Close equals the lowest low in the lookback, Stoch should be exactly 0.
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <returns>List of bars where Close equals Low.</returns>
    internal static IReadOnlyList<Bar> GetCloseEqualsLow(int bars = 100)
    {
        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            decimal price = 100m + (i * 0.1m);
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: price + 1m,
                High: price + 2m,
                Low: price, // Close = Low
                Close: price, // Close = Low
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates bars where Close = High = Low (flat candles).
    /// This should produce Stoch = 0 (by convention when highHigh - lowLow = 0).
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <returns>List of bars with flat candles.</returns>
    internal static IReadOnlyList<Bar> GetFlatCandles(int bars = 100)
    {
        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            decimal price = 100m + (i * 0.01m);
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: price,
                High: price,
                Low: price,
                Close: price,
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates alternating up/down bars for CMO boundary testing.
    /// Alternating gains and losses of equal magnitude should produce CMO = 0.
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <returns>List of bars with alternating price movements.</returns>
    internal static IReadOnlyList<Bar> GetAlternating(int bars = 100)
    {
        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);
        const decimal basePrice = 100m;

        for (int i = 0; i < bars; i++)
        {
            decimal offset = (i % 2 == 0) ? 0m : 1m;
            decimal close = basePrice + offset;
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: close,
                High: close + 0.5m,
                Low: close - 0.5m,
                Close: close,
                Volume: 1000m));
        }

        return series;
    }

    /// <summary>
    /// Generates bars with extremely small price movements to test precision limits.
    /// Uses values near the limits of double precision (around 1e-14 relative error).
    /// </summary>
    /// <param name="bars">Number of bars to generate.</param>
    /// <returns>List of bars with tiny price movements.</returns>
    internal static IReadOnlyList<Bar> GetTinyMovements(int bars = 100)
    {
        // Large base price to magnify floating-point precision issues
        // When divided, small differences relative to this base expose precision limits
        const decimal largePriceBase = 1000000m;
        const decimal tinyIncrement = 0.0000001m;

        List<Bar> series = new(bars);
        DateTime timestamp = DateTime.Today.AddDays(-bars);

        for (int i = 0; i < bars; i++)
        {
            // Very small increments that exercise floating-point precision
            decimal close = largePriceBase + (i * tinyIncrement);
            series.Add(new Bar(
                Timestamp: timestamp.AddDays(i),
                Open: close,
                High: close + tinyIncrement,
                Low: close - tinyIncrement,
                Close: close,
                Volume: 1000m));
        }

        return series;
    }
}
