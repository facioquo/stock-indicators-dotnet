namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Incremental rolling highest or lowest value, in O(1) amortized time per value.
/// </summary>
/// <remarks>
/// Keeps a monotonic deque of candidate extremes in a ring that grows on demand
/// up to one window, so a large lookback on a short stream allocates little.
/// </remarks>
internal sealed class RollingExtremeWindow
{
    private readonly int _lookbackPeriods;
    private readonly bool _isMax;

    private long[] _positions;
    private double[] _values;
    private int _head;
    private int _count;
    private long _position = -1;
    private long _lastNaN = -1;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingExtremeWindow"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <param name="isMax">Whether to track the highest (true) or lowest (false) value.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    internal RollingExtremeWindow(int lookbackPeriods, bool isMax)
    {
        ResultMath.ValidateLookback(lookbackPeriods);
        _lookbackPeriods = lookbackPeriods;
        _isMax = isMax;

        int capacity = Math.Min(lookbackPeriods, 16);
        _positions = new long[capacity];
        _values = new double[capacity];
    }

    /// <summary>
    /// Advances the window by one value.
    /// </summary>
    /// <param name="value">Next value in the series.</param>
    /// <returns>
    /// The window extreme; NaN before the window fills and while it holds a NaN value.
    /// </returns>
    internal double Add(double value)
    {
        _position++;

        if (double.IsNaN(value))
        {
            _lastNaN = _position;
            _head = 0;
            _count = 0;
        }
        else
        {
            // evict the expired head first, so the push below never exceeds one window
            if (_count > 0 && _positions[_head] <= _position - _lookbackPeriods)
            {
                _head = (_head + 1) % _positions.Length;
                _count--;
            }

            while (_count > 0)
            {
                double back = _values[(_head + _count - 1) % _values.Length];

                if (_isMax ? back > value : back < value)
                {
                    break;
                }

                _count--;
            }

            if (_count == _positions.Length)
            {
                Grow();
            }

            int tail = (_head + _count) % _positions.Length;
            _positions[tail] = _position;
            _values[tail] = value;
            _count++;
        }

        return _position < _lookbackPeriods - 1 || _lastNaN > _position - _lookbackPeriods
            ? double.NaN
            : _values[_head];
    }

    /// <summary>
    /// Resets the window to its initial, empty state.
    /// </summary>
    internal void Clear()
    {
        _head = 0;
        _count = 0;
        _position = -1;
        _lastNaN = -1;
    }

    /// <summary>
    /// Doubles the ring capacity, up to one window, keeping deque order.
    /// </summary>
    private void Grow()
    {
        int capacity = Math.Min(_lookbackPeriods, _positions.Length * 2);
        long[] positions = new long[capacity];
        double[] values = new double[capacity];

        for (int k = 0; k < _count; k++)
        {
            int from = (_head + k) % _positions.Length;
            positions[k] = _positions[from];
            values[k] = _values[from];
        }

        _positions = positions;
        _values = values;
        _head = 0;
    }
}

/// <summary>
/// Incremental rolling median over a sorted window.
/// </summary>
/// <remarks>
/// Each value costs O(log k) comparisons plus an O(k) shift for a lookback of k periods.
/// </remarks>
internal sealed class RollingMedianWindow
{
    private readonly int _lookbackPeriods;
    private readonly Queue<double> _arrivals = new();

    // window values in ascending order (NaN sorts first)
    private readonly List<double> _sorted = [];
    private int _nanCount;

    /// <summary>
    /// Initializes a new instance of the <see cref="RollingMedianWindow"/> class.
    /// </summary>
    /// <param name="lookbackPeriods">Number of periods in the window, including the current one.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the lookback periods are less than 1.</exception>
    internal RollingMedianWindow(int lookbackPeriods)
    {
        ResultMath.ValidateLookback(lookbackPeriods);
        _lookbackPeriods = lookbackPeriods;
    }

    /// <summary>
    /// Advances the window by one value.
    /// </summary>
    /// <param name="value">Next value in the series.</param>
    /// <returns>
    /// The window median, averaging the two middle values when the window is even;
    /// NaN before the window fills and while it holds a NaN value.
    /// </returns>
    internal double Add(double value)
    {
        _arrivals.Enqueue(value);
        int index = _sorted.BinarySearch(value);
        _sorted.Insert(index < 0 ? ~index : index, value);

        if (double.IsNaN(value))
        {
            _nanCount++;
        }

        if (_arrivals.Count > _lookbackPeriods)
        {
            double outgoing = _arrivals.Dequeue();
            _sorted.RemoveAt(_sorted.BinarySearch(outgoing));

            if (double.IsNaN(outgoing))
            {
                _nanCount--;
            }
        }

        if (_arrivals.Count < _lookbackPeriods || _nanCount > 0)
        {
            return double.NaN;
        }

        int mid = _lookbackPeriods / 2;

        return _lookbackPeriods % 2 == 0
            ? Midpoint(_sorted[mid - 1], _sorted[mid])
            : _sorted[mid];
    }

    /// <summary>
    /// Resets the window to its initial, empty state.
    /// </summary>
    internal void Clear()
    {
        _arrivals.Clear();
        _sorted.Clear();
        _nanCount = 0;
    }

    /// <summary>
    /// Averages two values, exactly when their sum is finite and without overflow when it is not.
    /// </summary>
    /// <param name="a">First value.</param>
    /// <param name="b">Second value.</param>
    /// <returns>The value halfway between <paramref name="a"/> and <paramref name="b"/>.</returns>
    private static double Midpoint(double a, double b)
    {
        double sum = a + b;

        return double.IsInfinity(sum) && double.IsFinite(a) && double.IsFinite(b)
            ? (a / 2d) + (b / 2d)
            : sum / 2d;
    }
}
