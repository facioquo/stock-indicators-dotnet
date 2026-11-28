namespace FacioQuo.Stock.Indicators;

/// <summary>
/// A hub's result cache and internal indicator state at one point in time.
/// </summary>
/// <remarks>
/// <para>
/// Restoring a snapshot into a hub with the same name and an aligned provider
/// resumes streaming without replaying the source history.
/// </para>
/// <para>
/// A snapshot can be persisted with any serializer that round-trips a positional record,
/// such as <c>System.Text.Json</c>. Treat <see cref="State"/> as opaque bytes.
/// A restore rejects a snapshot whose <see cref="FormatVersion"/> or encoded state version
/// the running library does not support, by throwing <see cref="InvalidDataException"/>;
/// it never migrates an older snapshot. After such a rejection, rebuild the hub from history.
/// </para>
/// </remarks>
/// <typeparam name="TOut">Type of the hub's results.</typeparam>
/// <param name="FormatVersion">Version of this envelope; a restore rejects any other value.</param>
/// <param name="HubName">Name of the hub that captured the snapshot, such as <c>RSI(14)</c>.</param>
/// <param name="PrunedThrough">Newest timestamp the hub has pruned, which later input may not predate.</param>
/// <param name="Results">Copy of the hub's result cache.</param>
/// <param name="State">The hub's internal state, in the hub's own versioned encoding.</param>
public sealed record HubSnapshot<TOut>(
    int FormatVersion,
    string HubName,
    DateTime? PrunedThrough,
    IReadOnlyList<TOut> Results,
    ReadOnlyMemory<byte> State);

/// <summary>
/// Version of the <see cref="HubSnapshot{TOut}"/> envelope.
/// </summary>
internal static class HubSnapshot
{
    /// <summary>
    /// Current envelope version.
    /// </summary>
    internal const int FormatVersion = 1;
}
