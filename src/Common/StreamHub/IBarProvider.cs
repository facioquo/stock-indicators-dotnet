namespace FacioQuo.Stock.Indicators;

/// <inheritdoc/>
public interface IBarProvider<out T> : IChainProvider<T>
   where T : IBar
{
    /// <summary>
    /// Gets the read-only list of bars.
    /// </summary>
    IReadOnlyList<T> Bars { get; }
}
