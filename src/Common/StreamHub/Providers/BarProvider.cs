namespace FacioQuo.Stock.Indicators;

/// <inheritdoc cref="IStreamHub{TIn, TOut}"/>
public abstract class BarProvider<TIn, TOut> : StreamHub<TIn, TOut>, IBarProvider<TOut>
     where TIn : IReusable
     where TOut : IBar
{
    /// <inheritdoc cref="IStreamHub{TIn, TOut}"/>
    /// <param name="provider">Streaming data provider.</param>
    internal BarProvider(
        IStreamObservable<TIn> provider
    ) : base(provider)
    => Bars = Cache.AsReadOnly();  // instantiate once

    /// <summary>
    /// Gets the bars as a read-only collection (safe from external mutation).
    /// </summary>
    public IReadOnlyList<TOut> Bars { get; }
}
