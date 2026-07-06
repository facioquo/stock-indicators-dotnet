#nullable enable

namespace Behavioral;

/// <summary>
/// BufferList convergence tests verify that BufferList implementations
/// produce consistent results with different amounts of historical data
/// and match Series calculations at convergence point.
/// </summary>
/// <remarks>
/// Convergence tests are the only kind of test allowed to use
/// <c>BeApproximately()</c> precision instead of exact matching.
/// </remarks>
[TestClass, TestCategory("Integration")]
public class ConvergenceBufferLists : TestBaseWithPrecision
{
    private static readonly int[] BarsQuantities =
        [5, 14, 28, 40, 50, 75, 100, 110, 120, 130, 140, 150, 160, 175, 200, 250, 350, 500, 600, 700, 800, 900, 1000];

    // Convergence thresholds: quantity needed for stable final value
    private const int AdxConvergence = 350;
    private const int AtrConvergence = 200;
    private const int EmaConvergence = 150;
    private const int MacdConvergence = 250;
    private const int RsiConvergence = 200;
    private const int SmaConvergence = 50;
    private const int StochConvergence = 200;

    private const double MaxChange = 0.0001;

    [TestMethod]
    public void Adx()
    {
        const int lookback = 14;
        AdxResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            AdxList buffer = qts.ToAdxList(lookback);

            AdxResult l = buffer[^1];
            Console.WriteLine($"ADX({lookback}) BufferList on {l.Timestamp:d} with {qty,4} historical qts: {l.Adx:N8}");

            // At convergence point, verify matches Series and save for stability check
            if (qty == AdxConvergence)
            {
                IReadOnlyList<AdxResult> series = qts.ToAdx(lookback);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify result is stable (within precision threshold)
            if (qty > AdxConvergence && convergedResult != null)
            {
                l.Adx.Should().BeApproximately(convergedResult.Adx!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Atr()
    {
        const int lookback = 14;
        AtrResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            AtrList buffer = qts.ToAtrList(lookback);

            AtrResult l = buffer[^1];
            Console.WriteLine($"ATR({lookback}) BufferList on {l.Timestamp:d} with {qty,4} periods: {l.Atr:N8}");

            // At convergence point, verify matches Series
            if (qty == AtrConvergence)
            {
                IReadOnlyList<AtrResult> series = qts.ToAtr(lookback);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > AtrConvergence && convergedResult != null)
            {
                l.Atr.Should().BeApproximately(convergedResult.Atr!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Ema()
    {
        const int lookback = 14;
        EmaResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            EmaList buffer = qts.ToEmaList(lookback);

            EmaResult l = buffer[^1];
            Console.WriteLine($"EMA({lookback}) BufferList on {l.Timestamp:d} with {qty,4} periods: {l.Ema:N8}");

            // At convergence point, verify matches Series
            if (qty == EmaConvergence)
            {
                IReadOnlyList<EmaResult> series = qts.ToEma(lookback);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > EmaConvergence && convergedResult != null)
            {
                l.Ema.Should().BeApproximately(convergedResult.Ema!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Macd()
    {
        const int fast = 12, slow = 26, signal = 9;
        MacdResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            MacdList buffer = qts.ToMacdList(fast, slow, signal);

            MacdResult l = buffer[^1];
            Console.WriteLine($"MACD BufferList on {l.Timestamp:d} with {qty,4} periods: {l.Macd:N8}");

            // At convergence point, verify matches Series
            if (qty == MacdConvergence)
            {
                IReadOnlyList<MacdResult> series = qts.ToMacd(fast, slow, signal);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > MacdConvergence && convergedResult != null)
            {
                l.Macd.Should().BeApproximately(convergedResult.Macd!.Value, MaxChange);
                l.Signal.Should().BeApproximately(convergedResult.Signal!.Value, MaxChange);
                l.Histogram.Should().BeApproximately(convergedResult.Histogram!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Rsi()
    {
        const int lookback = 14;
        RsiResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            RsiList buffer = qts.ToRsiList(lookback);

            RsiResult l = buffer[^1];
            Console.WriteLine($"RSI({lookback}) BufferList on {l.Timestamp:d} with {qty,4} periods: {l.Rsi:N8}");

            // At convergence point, verify matches Series
            if (qty == RsiConvergence)
            {
                IReadOnlyList<RsiResult> series = qts.ToRsi(lookback);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > RsiConvergence && convergedResult != null)
            {
                l.Rsi.Should().BeApproximately(convergedResult.Rsi!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Sma()
    {
        const int lookback = 14;
        SmaResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            SmaList buffer = qts.ToSmaList(lookback);

            SmaResult l = buffer[^1];
            Console.WriteLine($"SMA({lookback}) BufferList on {l.Timestamp:d} with {qty,4} periods: {l.Sma:N8}");

            // At convergence point, verify matches Series
            if (qty == SmaConvergence)
            {
                IReadOnlyList<SmaResult> series = qts.ToSma(lookback);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > SmaConvergence && convergedResult != null)
            {
                l.Sma.Should().BeApproximately(convergedResult.Sma!.Value, MaxChange);
            }
        }
    }

    [TestMethod]
    public void Stoch()
    {
        const int lookbackPeriods = 14, signalPeriods = 3, smoothPeriods = 3;
        StochResult? convergedResult = null;

        foreach (int qty in BarsQuantities)
        {
            IReadOnlyList<Bar> qts = Data.GetLongish(qty);
            StochList buffer = qts.ToStochList(lookbackPeriods, signalPeriods, smoothPeriods);

            StochResult l = buffer[^1];
            Console.WriteLine($"STOCH BufferList on {l.Timestamp:d} with {qty,4} periods: {l.K:N8}");

            // At convergence point, verify matches Series
            if (qty == StochConvergence)
            {
                IReadOnlyList<StochResult> series = qts.ToStoch(lookbackPeriods, signalPeriods, smoothPeriods);
                l.Should().Be(series[^1]);
                convergedResult = l;
            }

            // After convergence, verify stability
            if (qty > StochConvergence && convergedResult != null)
            {
                l.K.Should().BeApproximately(convergedResult.K!.Value, MaxChange);
                l.D.Should().BeApproximately(convergedResult.D!.Value, MaxChange);
            }
        }
    }
}
