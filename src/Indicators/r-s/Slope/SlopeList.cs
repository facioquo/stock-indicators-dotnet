namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Slope and linear regression from incremental reusable values.
/// </summary>
/// <remarks>
/// Performance optimizations:
/// - Pre-calculates sumSqX constant (variance of sequential X values)
/// - Calculates sumX mathematically instead of iterating
/// - Minimizes Line value updates to only necessary items
/// <para>
/// Two-pass deviation form (avgY pass, then sumSqY/sumSqXy pass) is retained because
/// the textbook running-sums identity <c>sumSqY = Σy² − (Σy)²/n</c> cancels
/// catastrophically when <c>Yi ≈ avgY</c>, producing ULP-level drift against the
/// Series oracle and failing the BufferList <c>IsExactly</c> bit-equality contract.
/// Numerically-stable O(1) sliding-window variants (Welford/Pébay) were not evaluated.
/// </para>
/// </remarks>
public class SlopeList : BufferList<SlopeResult>, IIncrementFromChain, ISlope
{
    private readonly Queue<double> _buffer;
    private readonly int lookbackPeriods;

    // Tracks how many items have been removed from the beginning
    private int globalIndexOffset;

    // Pre-calculated constant for X variance (sequential integers).
    // Formula: n*(n²-1)/12 where n = lookbackPeriods
    private readonly double sumSqXConstant;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlopeList"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SlopeList(int lookbackPeriods)
    {
        Slope.Validate(lookbackPeriods);
        this.lookbackPeriods = lookbackPeriods;
        LookbackPeriods = lookbackPeriods;
        _buffer = new Queue<double>(lookbackPeriods);
        globalIndexOffset = 0;

        // Pre-calculate constant sumSqX for sequential X values
        // When X values are [x, x+1, ..., x+n-1], avgX = x + (n-1)/2
        // Sum of (Xi - avgX)^2 = n*(n^2 - 1)/12
        sumSqXConstant = lookbackPeriods * ((lookbackPeriods * lookbackPeriods) - 1) / 12.0;

        Name = $"SLOPE({lookbackPeriods})";
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SlopeList"/> class with initial values.
    /// </summary>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    /// <param name="values">Initial reusable values to populate the list.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="lookbackPeriods"/> is invalid.</exception>
    public SlopeList(
        int lookbackPeriods,
        IReadOnlyList<IReusable> values)
        : this(lookbackPeriods) => Add(values);

    /// <inheritdoc/>
    public int LookbackPeriods { get; init; }

    /// <inheritdoc />
    public void Add(DateTime timestamp, double value)
    {
        // Update the rolling buffer
        _buffer.Update(lookbackPeriods, value);

        // During initialization period
        if (_buffer.Count < lookbackPeriods)
        {
            AddInternal(new SlopeResult(timestamp));
            return;
        }

        // The current global index (0-based, but we use 1-based for X values)
        int currentIndex = globalIndexOffset + Count;

        // Optimization: Calculate X values mathematically (sequential integers)
        // X values are: (currentIndex - lookbackPeriods + 2) to (currentIndex + 1)
        // For sequential X = [a, a+1, ..., a+n-1]:
        // - sumX = n*a + n*(n-1)/2
        // - avgX = a + (n-1)/2
        double firstX = currentIndex - lookbackPeriods + 2d;
        double sumX = (lookbackPeriods * firstX) + (lookbackPeriods * (lookbackPeriods - 1) / 2.0);
        double avgX = sumX / lookbackPeriods;

        // Calculate sums for least squares method
        // Two passes required: 1) get avgY, 2) calculate deviations

        // First pass: calculate avgY
        double sumY = 0;
        foreach (double bufferValue in _buffer)
        {
            sumY += bufferValue;
        }

        double avgY = sumY / _buffer.Count;

        // Second pass: calculate deviations and their products
        double sumSqY = 0;
        double sumSqXy = 0;
        int relativeIndex = 0;
        foreach (double bufferValue in _buffer)
        {
            double xValue = firstX + relativeIndex;
            double devX = xValue - avgX;
            double devY = bufferValue - avgY;

            sumSqY += devY * devY;
            sumSqXy += devX * devY;
            relativeIndex++;
        }

        // Use pre-calculated constant for sumSqX
        double? slope = (sumSqXy / sumSqXConstant).NaN2Null();
        double? intercept = (avgY - (slope * avgX)).NaN2Null();

        // Calculate Standard deviation and R-squared
        double stdDevX = Math.Sqrt(sumSqXConstant / lookbackPeriods);
        double stdDevY = Math.Sqrt(sumSqY / lookbackPeriods);

        double? rSquared = null;

        if (stdDevX * stdDevY != 0)
        {
            double arrr = sumSqXy / (stdDevX * stdDevY) / lookbackPeriods;
            rSquared = (arrr * arrr).NaN2Null();
        }

        // Write result (Line will be calculated after we have the current slope)
        SlopeResult result = new(
            Timestamp: timestamp,
            Slope: slope,
            Intercept: intercept,
            StdDev: stdDevY.NaN2Null(),
            RSquared: rSquared,
            Line: null);

        AddInternal(result);

        // Update Line values for the last lookbackPeriods results
        // This is legitimate historical repaint matching Series behavior
        UpdateLineValues();
    }

    /// <inheritdoc />
    public void Add(IReusable value)
    {
        ArgumentNullException.ThrowIfNull(value);
        Add(value.Timestamp, value.Value);
    }

    /// <inheritdoc />
    public void Add(IReadOnlyList<IReusable> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        for (int i = 0; i < values.Count; i++)
        {
            Add(values[i].Timestamp, values[i].Value);
        }
    }

    /// <inheritdoc />
    public override void Clear()
    {
        base.Clear();
        _buffer.Clear();
        globalIndexOffset = 0;
    }
    /// <summary>
    /// Overrides list pruning to update the global index offset.
    /// </summary>
    protected override void PruneList()
    {
        int countBeforePrune = Count;
        base.PruneList();
        int countAfterPrune = Count;

        // Update offset by the number of items removed
        globalIndexOffset += countBeforePrune - countAfterPrune;
    }

    /// <summary>
    /// Updates Line values for the last lookbackPeriods results using the most recent slope/intercept.
    /// This is legitimate historical repaint behavior matching the Series implementation.
    /// Optimized: Only updates the necessary items to minimize overhead.
    /// </summary>
    private void UpdateLineValues()
    {
        // Only update if we have enough results
        if (Count < lookbackPeriods)
        {
            return;
        }

        // Get the most recent result (just added) which has the current slope/intercept
        SlopeResult lastResult = this[Count - 1];

        // Optimization: Only nullify the one item that just fell out of the window
        // (when Count > lookbackPeriods, the item at index Count - lookbackPeriods - 1 just left the window)
        if (Count > lookbackPeriods)
        {
            int itemToNullify = Count - lookbackPeriods - 1;
            SlopeResult existing = this[itemToNullify];
            if (existing.Line is not null)
            {
                SlopeResult updated = existing with { Line = null };
                UpdateInternal(itemToNullify, updated);
            }
        }

        // Update Line values for the last lookbackPeriods results
        // Using global indices (globalIndex + 1) like the series implementation
        int startIndex = Count - lookbackPeriods;
        for (int p = startIndex; p < Count; p++)
        {
            SlopeResult existing = this[p];

            // Calculate Line: y = mx + b, using global index (offset + p + 1)
            int globalIndex = globalIndexOffset + p;
            decimal? line = (decimal?)((lastResult.Slope * (globalIndex + 1)) + lastResult.Intercept).NaN2Null();

            SlopeResult updated = existing with { Line = line };
            UpdateInternal(p, updated);
        }
    }
}

public static partial class Slope
{
    /// <summary>
    /// Creates a buffer list for Slope calculations.
    /// </summary>
    /// <param name="source">Collection of input values, time sorted.</param>
    /// <param name="lookbackPeriods">Quantity of periods in lookback window.</param>
    public static SlopeList ToSlopeList(
        this IReadOnlyList<IReusable> source,
        int lookbackPeriods = 14)
        => new(lookbackPeriods) { source };
}
