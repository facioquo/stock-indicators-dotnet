namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides methods for calculating the Fractal Chaos Bands (FCB) using a stream hub.
/// </summary>
public class FcbHub
    : StreamHub<IBar, FcbResult>, IFcb
{
    internal FcbHub(
        IBarProvider<IBar> provider,
        int windowSpan) : base(provider)
    {
        Fcb.Validate(windowSpan);
        WindowSpan = windowSpan;
        Name = $"FCB({windowSpan})";

        // Validate cache size for warmup requirements
        ValidateCacheSize(2 * windowSpan, Name);

        Reinitialize();
    }

    /// <inheritdoc/>
    public int WindowSpan { get; init; }

    /// <summary>
    /// state tracking for upper and lower bands
    /// </summary>
    private decimal? UpperLine { get; set; }
    private decimal? LowerLine { get; set; }

    /// <inheritdoc/>
    protected override (FcbResult result, int index)
        ToIndicator(IBar item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        // check if we can identify fractals (need full window)
        if (i >= 2 * WindowSpan)
        {
            // look at the fractal from windowSpan periods ago
            int fractalIndex = i - WindowSpan;
            IBar fractalBar = ProviderCache[fractalIndex];

            // check for bearish fractal (high point) at fractalIndex
            bool isBearishFractal = true;
            for (int p = fractalIndex - WindowSpan; p <= fractalIndex + WindowSpan; p++)
            {
                if (p != fractalIndex && ProviderCache[p].High >= fractalBar.High)
                {
                    isBearishFractal = false;
                    break;
                }
            }

            // check for bullish fractal (low point) at fractalIndex
            bool isBullishFractal = true;
            for (int p = fractalIndex - WindowSpan; p <= fractalIndex + WindowSpan; p++)
            {
                if (p != fractalIndex && ProviderCache[p].Low <= fractalBar.Low)
                {
                    isBullishFractal = false;
                    break;
                }
            }

            // update lines based on detected fractals
            if (isBearishFractal)
            {
                UpperLine = fractalBar.High;
            }

            if (isBullishFractal)
            {
                LowerLine = fractalBar.Low;
            }
        }

        // create result
        FcbResult r = new(
            Timestamp: item.Timestamp,
            UpperBand: UpperLine,
            LowerBand: LowerLine);

        return (r, i);
    }

    /// <summary>
    /// Restores the prior FCB state.
    /// </summary>
    /// <inheritdoc/>
    protected override void RollbackState(int restoreIndex)
    {
        // restore prior state
        if (restoreIndex >= 0)
        {
            FcbResult priorResult = Cache[restoreIndex];
            UpperLine = priorResult.UpperBand;
            LowerLine = priorResult.LowerBand;
        }
        else
        {
            // reset to initial state
            UpperLine = null;
            LowerLine = null;
        }
    }
}

public static partial class Fcb
{
    /// <summary>
    /// Creates a Fractal Chaos Bands (FCB) hub.
    /// </summary>
    /// <param name="barProvider">Bar provider.</param>
    /// <param name="windowSpan">Window span used for fractal detection.</param>
    /// <returns>An instance of <see cref="FcbHub"/>.</returns>
    public static FcbHub ToFcbHub(
       this IBarProvider<IBar> barProvider,
       int windowSpan = 2)
           => new(barProvider, windowSpan);
}
