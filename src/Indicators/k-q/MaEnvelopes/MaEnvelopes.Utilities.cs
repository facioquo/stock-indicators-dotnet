namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for the Moving Average Envelopes.
/// </summary>
public static partial class MaEnvelopes
{
    /// <summary>
    /// Removes empty (null) periods from the Moving Average Envelope results.
    /// </summary>
    /// <param name="results">List of Moving Average Envelope results.</param>
    /// <returns>A list of Moving Average Envelope results with empty periods removed.</returns>
    public static IReadOnlyList<MaEnvelopeResult> Condense(
        this IReadOnlyList<MaEnvelopeResult> results)
    {
        List<MaEnvelopeResult> resultsList = results
            .ToList();

        resultsList
            .RemoveAll(match:
                static x => x.UpperEnvelope is null && x.LowerEnvelope is null && x.Centerline is null);

        return resultsList.ToSortedList();
    }

    /// <summary>
    /// Validates the parameters for the Moving Average Envelopes calculation.
    /// </summary>
    /// <param name="percentOffset">Percentage offset for the envelopes.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the percent offset is less than or equal to 0.</exception>
    internal static void Validate(
        double percentOffset)
    {
        // check parameter arguments
        if (percentOffset <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(percentOffset), percentOffset,
                "Percent Offset must be greater than 0 for Moving Average Envelopes.");
        }
    }
}
