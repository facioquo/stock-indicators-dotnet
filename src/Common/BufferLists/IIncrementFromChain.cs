namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Interface for incrementing with <see cref="IReusable"/> values.
/// </summary>
/// <remarks>
/// This patterns supports chainable indicators that can be calculated from single values.
/// Examples: TR, RSI, moving averages, and others.
/// </remarks>
internal interface IIncrementFromChain
{
    /// <summary>
    /// Apply new reusable input value to increment indicator list values.
    /// </summary>
    /// <remarks>
    /// Values must be supplied in chronological order. This method assumes the
    /// new value is newer than every previously added value; it does not detect
    /// or correct out-of-order, duplicate, or revised timestamps. Use a
    /// <c>StreamHub</c> when input can arrive out of order.
    /// </remarks>
    /// <param name="timestamp">Date context, newer than the last added.</param>
    /// <param name="value">Next value.</param>
    void Add(DateTime timestamp, double value);

    /// <summary>
    /// Apply new input value to increment indicator list values.
    /// </summary>
    /// <remarks>
    /// Values must be supplied in chronological order; see
    /// <see cref="Add(DateTime, double)"/> for the ordering contract.
    /// </remarks>
    /// <param name="value">Next reusable value, newer than the last added.</param>
    void Add(IReusable value);

    /// <summary>
    /// Apply batch of reusable input values to increment many indicator list values.
    /// </summary>
    /// <param name="values">A chronologically ordered batch of <see cref="IReusable"/> values.</param>
    void Add(IReadOnlyList<IReusable> values);
}
