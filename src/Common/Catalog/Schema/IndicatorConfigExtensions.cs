namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Extension methods for working with indicator configurations.
/// </summary>
public static partial class IndicatorConfigExtensions
{
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
