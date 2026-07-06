using System.Collections.ObjectModel;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Represents a strongly typed collection of StreamHub objects.
/// </summary>
/// <remarks>
/// <para>
/// This collection stores hubs using the covariant <see cref="IStreamObservable{T}"/> interface
/// to support heterogeneous hub types (e.g., EmaHub, RsiHub, BarHub) in a single collection.
/// All concrete hub implementations inherit from <see cref="StreamHub{TIn, TOut}"/> which implements
/// both <see cref="IStreamHub{TIn, TOut}"/> and <see cref="IStreamObservable{T}"/>.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// BarHub barHub = new();
///
/// HubCollection hubs =
/// [
///     barHub.ToEmaHub(12),
///     barHub.ToEmaHub(26),
///     barHub.ToRsiHub(14)
/// ];
///
/// // Access results from all hubs
/// IEnumerable&lt;IReadOnlyList&lt;ISeries&gt;&gt; results = hubs.Results;
/// var latestEma12 = results.ElementAt(0)[^1];
///
/// // Get last values from all hubs
/// IEnumerable&lt;double&gt; lastValues = hubs.LastValues;
/// double ema12Value = lastValues.ElementAt(0);
/// </code>
/// </example>
public class HubCollection : Collection<IStreamObservable<ISeries>>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="HubCollection"/> class that is empty.
    /// </summary>
    public HubCollection() { }

    /// <summary>
    /// Initializes a new instance of the <see cref="HubCollection"/> class
    /// that contains elements copied from the specified collection.
    /// </summary>
    /// <param name="collection">Collection whose elements are copied to the new collection.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="collection"/> is null.</exception>
    public HubCollection(IEnumerable<IStreamObservable<ISeries>> collection)
        : base(new List<IStreamObservable<ISeries>>(
            collection
                ?? throw new ArgumentNullException(nameof(collection))))
    { }

    /// <summary>
    /// Gets the collection of result sets produced by each hub in the sequence.
    /// </summary>
    public IEnumerable<IReadOnlyList<ISeries>> Results => this.Select(hub => hub.Results);

    /// <summary>
    /// Gets the last value from each hub's results.
    /// </summary>
    /// <returns>
    /// An enumerable of double values where each element corresponds to a hub in the collection.
    /// Returns the <see cref="IReusable.Value"/> of the last result for hubs with <see cref="IReusable"/> results,
    /// or <see cref="double.NaN"/> for hubs with non-reusable results or empty result sets.
    /// </returns>
    public IEnumerable<double> LastValues => this
        .Select(hub
            => hub.Results is IReadOnlyList<IReusable> reusableResults
            && reusableResults.Count > 0
                ? reusableResults[^1].Value
                : double.NaN);
}
