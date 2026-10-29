namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for TTM Squeeze streaming and buffered list.
/// </summary>
public interface ITtmSqueeze
{
    /// <summary>
    /// Gets the number of periods for the bands, channels, and momentum.
    /// </summary>
    int LookbackPeriods { get; }

    /// <summary>
    /// Gets the width of the Bollinger Bands, in standard deviations.
    /// </summary>
    double StandardDeviations { get; }

    /// <summary>
    /// Gets the width of the Keltner Channels, in average true ranges.
    /// </summary>
    double Multiplier { get; }
}
