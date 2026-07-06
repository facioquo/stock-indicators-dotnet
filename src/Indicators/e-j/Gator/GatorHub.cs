namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Streaming hub for Gator Oscillator.
/// </summary>
public class GatorHub
   : StreamHub<AlligatorResult, GatorResult>
{
    internal GatorHub(IChainProvider<IReusable> chainProvider)
        : this(chainProvider.ToAlligatorHub()) { }

    internal GatorHub(AlligatorHub alligatorHub)
        : base(alligatorHub)
    {
        ArgumentNullException.ThrowIfNull(alligatorHub);
        Name = "GATOR()";

        // Validate cache size for warmup requirements
        // Gator inherits from Alligator, validation not needed here
        // as AlligatorHub already validates

        Reinitialize();
    }

    /// <inheritdoc/>
    protected override (GatorResult result, int index)
        ToIndicator(AlligatorResult item, int? indexHint)
    {
        ArgumentNullException.ThrowIfNull(item);

        int i = indexHint ?? ProviderCache.IndexOf(item, true);

        if (i == 0)
        {
            // First result is always null values
            GatorResult r = new(item.Timestamp);
            return (r, i);
        }

        GatorResult p = Cache[i - 1];

        double? upper = (item.Jaw - item.Teeth).Abs();
        double? lower = -(item.Teeth - item.Lips).Abs();

        GatorResult result = new(
            Timestamp: item.Timestamp,
            Upper: upper,
            Lower: lower,
            UpperIsExpanding: p.Upper is not null ? upper > p.Upper : null,
            LowerIsExpanding: p.Lower is not null ? lower < p.Lower : null);

        return (result, i);
    }
}

public static partial class Gator
{
    /// <summary>
    /// Creates a Gator hub from a chain provider.
    /// </summary>
    /// <param name="chainProvider">Chain provider.</param>
    /// <returns>A Gator hub.</returns>
    public static GatorHub ToGatorHub(
        this IChainProvider<IReusable> chainProvider)
        => new(chainProvider);

    /// <summary>
    /// Creates a new Gator hub, using values from an existing Alligator hub.
    /// </summary>
    /// <param name="alligatorHub">Alligator hub.</param>
    /// <returns>A Gator hub.</returns>
    /// <remarks>
    /// <para>IMPORTANT: This is not a normal chaining approach.</para>
    /// This extension overrides and enables a chain that specifically
    /// reuses the existing <see cref="AlligatorHub"/> in its internal construction.
    ///</remarks>
    public static GatorHub ToGatorHub(
        this AlligatorHub alligatorHub)
        => new(alligatorHub);
}
