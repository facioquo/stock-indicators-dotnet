using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Extension methods for working with indicator configurations.
/// </summary>
public static class IndicatorConfigExtensions
{
    /// <summary>
    /// Obsolete. Permanently shadowed by the <see cref="IndicatorConfig.ToBuilder"/> instance
    /// method, which always takes precedence over this same-named extension method. Removal
    /// tracked in issue #2139.
    /// </summary>
    /// <param name="config">Indicator configuration.</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    [ExcludeFromCodeCoverage]
    [Obsolete("This extension method is permanently shadowed by the `IndicatorConfig.ToBuilder()` instance method and will be removed.", false)]
    public static ListingExecutionBuilder ToBuilder(this IndicatorConfig config)
    {
        ArgumentNullException.ThrowIfNull(config);

        // not recursive: C# overload resolution binds this call to the
        // IndicatorConfig.ToBuilder() instance method, which always takes
        // precedence over this same-named extension method
        return config.ToBuilder();
    }

    /// <summary>
    /// Executes an indicator configuration with the provided bars.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="config">Indicator configuration.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>Indicator results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when config is null.</exception>
    public static IReadOnlyList<TResult> Execute<TResult>(this IndicatorConfig config, IEnumerable<IBar> bars)
        where TResult : class
    {
        ArgumentNullException.ThrowIfNull(config);
        return config.ToBuilder().FromSource(bars).Execute<TResult>();
    }
}
