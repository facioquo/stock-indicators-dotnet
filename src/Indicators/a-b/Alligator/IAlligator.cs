namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for Alligator Hub.
/// </summary>
public interface IAlligator
{
    /// <summary>
    /// Gets the number of periods for the jaw.
    /// </summary>
    int JawPeriods { get; }

    /// <summary>
    /// Gets the offset for the jaw.
    /// </summary>
    int JawOffset { get; }

    /// <summary>
    /// Gets the number of periods for the teeth.
    /// </summary>
    int TeethPeriods { get; }

    /// <summary>
    /// Gets the offset for the teeth.
    /// </summary>
    int TeethOffset { get; }

    /// <summary>
    /// Gets the number of periods for the lips.
    /// </summary>
    int LipsPeriods { get; }

    /// <summary>
    /// Gets the offset for the lips.
    /// </summary>
    int LipsOffset { get; }
}
