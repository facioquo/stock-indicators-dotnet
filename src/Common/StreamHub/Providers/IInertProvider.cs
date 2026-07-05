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

/// <summary>
/// Inert provider for base Hub initialization.
/// It has no upstream data and cannot be observed.
/// Used only as a workaround for initializing a self-rooted hub
/// (e.g. <see cref="BarHub"/>, <see cref="TradeTickHub"/>) that has no
/// upstream provider. Scheduled for replacement by a dedicated
/// <c>StreamSource&lt;T&gt;</c> root in a future release.
/// </summary>
/// <typeparam name="T">Type of record</typeparam>
/// <remarks>
/// Only used to initialize a <see cref="BarHub"/> base that does not have its own provider.
/// </remarks>
/// <param name="maxCacheSize">Maximum cache size for the provider.</param>
internal class BaseProvider<T>(int maxCacheSize = 0)
    : IStreamObservable<T>, IInertProvider
    where T : IReusable
{
    private static readonly IReadOnlyList<T> _providerCache = Array.Empty<T>().AsReadOnly();

    /// <summary>
    /// Hub properties with non-standard defaults:
    /// bit 0 = 1 (disable observer) and mask = 0b11111110 (do not pass bit 0 to child hubs).
    /// </summary>
    /// <remarks>
    /// <see cref="BaseProvider{T}"/> is an inert provider that cannot observe.
    /// Bit 0 is set to 1 (disable observer) and masked to prevent child hubs from inheriting this restriction,
    /// allowing downstream hubs to be proper observers even though the base provider is not.
    /// <para>See <see cref="BinarySettings"/> for more information on bit settings and masks.</para>
    /// </remarks>
    public BinarySettings Properties { get; } = new(0b00000001, 0b11111110);

    /// <inheritdoc />
    /// <remarks>
    /// <see cref="BaseProvider{T}"/> does not have cached values."
    /// </remarks>
    public IReadOnlyList<T> Results => _providerCache;

    /// <inheritdoc/>
    public int MaxCacheSize { get; } = maxCacheSize;

    /// <inheritdoc/>
    public int MinCacheSize => 0; // Base provider has no minimum cache requirement

    /// <inheritdoc />
    public int ObserverCount => 0;

    /// <inheritdoc />
    public bool HasObservers
        => false; // No-op: base provider has no subscribers

    /// <inheritdoc />
    public bool HasSubscriber(IStreamObserver<T> observer)
        => false; // No-op: base provider has no subscribers

    /// <inheritdoc />
    public IDisposable Subscribe(IStreamObserver<T> observer)
        => throw new InvalidOperationException("Subscriptions not allowed on base provider.");

    /// <inheritdoc />
    public bool Unsubscribe(IStreamObserver<T> observer)
        => false; // No-op: base provider has no subscribers

    /// <inheritdoc />
    public void EndTransmission()
        => throw new InvalidOperationException("Base provider does not transmit to subscribers.");
}
