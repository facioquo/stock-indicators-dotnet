namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Marks an inert root provider — the placeholder backing a self-rooted
/// <see cref="BarHub"/> or <see cref="TradeTickHub"/> that has no upstream data.
/// A hub whose provider is inert is a <em>root</em> hub: it owns its own input
/// timeline and is the only kind of hub whose mutating API may be called
/// directly. Every hub that subscribes to a real provider is non-root and is
/// driven by that provider.
/// </summary>
internal interface IInertProvider;
