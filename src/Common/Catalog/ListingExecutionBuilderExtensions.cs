namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Extension methods for creating and working with custom indicator builders.
/// </summary>
public static class ListingExecutionBuilderExtensions
{
    /// <summary>
    /// Creates a customizable indicator builder from an indicator listing.
    /// </summary>
    /// <param name="listing">Base indicator listing.</param>
    /// <param name="parameterName">Name of the parameter</param>
    /// <param name="value">Value to set</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/> for fluent configuration.</returns>
    public static ListingExecutionBuilder WithParamValue(
        this IndicatorListing listing,
        string parameterName,
        object value)
            => new ListingExecutionBuilder(listing).WithParamValue(parameterName, value);

    /// <summary>
    /// Creates a customizable indicator builder from an indicator listing with multiple parameters.
    /// </summary>
    /// <param name="listing">Base indicator listing.</param>
    /// <param name="parameters">Dictionary of parameter names and values.</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/> for fluent configuration.</returns>
    public static ListingExecutionBuilder WithParams(
        this IndicatorListing listing,
        Dictionary<string, object> parameters)
            => new ListingExecutionBuilder(listing).WithParams(parameters);

    /// <summary>
    /// Creates a customizable indicator builder from an indicator listing and sets the source bars.
    /// </summary>
    /// <param name="listing">Base indicator listing.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/> with bars set.</returns>
    public static ListingExecutionBuilder From(
        this IndicatorListing listing,
        IEnumerable<IBar> bars)
            => new ListingExecutionBuilder(listing).FromSource(bars);

    /// <summary>
    /// Creates a customizable indicator builder from an indicator listing and sets a series source.
    /// </summary>
    /// <typeparam name="T">
    /// The type of elements in the series, which must implement <see cref="IReusable"/>.
    /// </typeparam>
    /// <param name="listing">Base indicator listing.</param>
    /// <param name="series">Series data to process.</param>
    /// <param name="parameterName">Name of the series parameter. If null, attempts to find the first series parameter.</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/> with the series parameter set.</returns>
    public static ListingExecutionBuilder From<T>(
        this IndicatorListing listing,
        IReadOnlyList<T> series,
        string? parameterName = null)
        where T : IReusable
        => new ListingExecutionBuilder(listing).FromSource(series, parameterName);

    /// <summary>
    /// Creates a customizable indicator builder from an indicator listing and sets a series source.
    /// Alias for From&lt;T&gt; to provide consistent naming with the fluent API.
    /// </summary>
    /// <typeparam name="T">Type of elements in the series, which must implement IReusable.</typeparam>
    /// <param name="listing">Base indicator listing.</param>
    /// <param name="series">Series data to process.</param>
    /// <param name="parameterName">Name of the series parameter. If null, attempts to find the first series parameter.</param>
    /// <returns>A <see cref="ListingExecutionBuilder"/> with the series parameter set.</returns>
    public static ListingExecutionBuilder FromSource<T>(
        this IndicatorListing listing,
        IReadOnlyList<T> series,
        string? parameterName = null)
        where T : IReusable
        => new ListingExecutionBuilder(listing).FromSource(series, parameterName);

    /// <summary>
    /// Executes an indicator using the default parameters from the catalog.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="listing">Indicator listing.</param>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <returns>Indicator results.</returns>
    public static IReadOnlyList<TResult> Execute<TResult>(
        this IndicatorListing listing,
        IEnumerable<IBar> bars)
        where TResult : class
        => ListingExecutor.Execute<TResult>(bars, listing);

    /// <summary>
    /// Alternative syntax: Execute an indicator from bars using a custom indicator builder.
    /// </summary>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="bars">Aggregate OHLCV price bars, time sorted.</param>
    /// <param name="customIndicator">Custom indicator configuration.</param>
    /// <returns>Indicator results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null</exception>
    public static IReadOnlyList<TResult> Execute<TResult>(
        this IEnumerable<IBar> bars,
        ListingExecutionBuilder customIndicator)
        where TResult : class
        => customIndicator == null
            ? throw new ArgumentNullException(nameof(customIndicator))
            : customIndicator.FromSource(bars).Execute<TResult>();

    /// <summary>
    /// Alternative syntax: Execute an indicator from a series using a custom indicator builder.
    /// </summary>
    /// <typeparam name="TSource">Type of elements in the source series, which must implement IReusable.</typeparam>
    /// <typeparam name="TResult">Expected result type.</typeparam>
    /// <param name="series">Series data to process.</param>
    /// <param name="customIndicator">Custom indicator configuration.</param>
    /// <param name="parameterName">Name of the series parameter. If null, attempts to find the first series parameter.</param>
    /// <returns>Indicator results.</returns>
    /// <exception cref="ArgumentNullException">Thrown when a required parameter is null</exception>
    public static IReadOnlyList<TResult> Execute<TSource, TResult>(
        this IReadOnlyList<TSource> series,
        ListingExecutionBuilder customIndicator,
        string? parameterName = null)
        where TSource : IReusable
        where TResult : class
        => customIndicator == null
            ? throw new ArgumentNullException(nameof(customIndicator))
            : customIndicator.FromSource(series, parameterName).Execute<TResult>();
}
