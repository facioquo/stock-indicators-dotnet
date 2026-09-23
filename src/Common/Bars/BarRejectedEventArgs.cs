namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Data for <see cref="BarHub.BarRejected"/>.
/// </summary>
/// <param name="bar">The refused bar.</param>
/// <param name="reason">Why the hub refused it.</param>
public sealed class BarRejectedEventArgs(IBar bar, BarRejectionReason reason) : EventArgs
{
    /// <summary>
    /// The refused bar.
    /// </summary>
    public IBar Bar { get; } = bar;

    /// <summary>
    /// Why the hub refused the bar.
    /// </summary>
    public BarRejectionReason Reason { get; } = reason;
}
