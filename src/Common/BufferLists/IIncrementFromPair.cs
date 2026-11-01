namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for incrementing with values from two series matched by timestamp.
/// </summary>
/// <remarks>
/// Each side is added separately and must be supplied in chronological order.
/// A result is added only once both sides hold a timestamp; a timestamp that only
/// one side holds produces no result.
/// </remarks>
internal interface IIncrementFromPair
{
    /// <summary>
    /// Adds the next value of the source series.
    /// </summary>
    /// <param name="timestamp">Date context, newer than the last source value.</param>
    /// <param name="value">Next source value.</param>
    void AddSource(DateTime timestamp, double value);

    /// <summary>
    /// Adds the next reusable value of the source series.
    /// </summary>
    /// <param name="value">Next source value, newer than the last added.</param>
    void AddSource(IReusable value);

    /// <summary>
    /// Adds a chronologically ordered batch of source values.
    /// </summary>
    /// <param name="values">Source values to add.</param>
    void AddSource(IReadOnlyList<IReusable> values);

    /// <summary>
    /// Adds the next value of the other series.
    /// </summary>
    /// <param name="timestamp">Date context, newer than the last other value.</param>
    /// <param name="value">Next other value.</param>
    void AddOther(DateTime timestamp, double value);

    /// <summary>
    /// Adds the next reusable value of the other series.
    /// </summary>
    /// <param name="value">Next other value, newer than the last added.</param>
    void AddOther(IReusable value);

    /// <summary>
    /// Adds a chronologically ordered batch of other values.
    /// </summary>
    /// <param name="values">Other values to add.</param>
    void AddOther(IReadOnlyList<IReusable> values);

    /// <summary>
    /// Adds the next value of both series at once.
    /// </summary>
    /// <param name="source">Next source value.</param>
    /// <param name="other">Next other value.</param>
    void Add(IReusable source, IReusable other);
}
