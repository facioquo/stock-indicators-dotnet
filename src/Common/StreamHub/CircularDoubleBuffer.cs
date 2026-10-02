namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fixed-size circular buffer for <c>double</c> values with O(capacity) max/min scan.
/// Zero heap allocation per tick; cache-line-resident for small windows (&lt;= ~32 elements).
/// </summary>
/// <remarks>
/// <see cref="GetMax"/> and <see cref="GetMin"/> return <see cref="double.NaN"/> when the buffer
/// is empty or when any value in the filled window is NaN, regardless of its position in the ring.
/// </remarks>
internal struct CircularDoubleBuffer
{
    private readonly double[] _values;
    private int _head;  // next write position
    private int _fill;  // count of valid entries (0..Capacity)

    internal CircularDoubleBuffer(int capacity)
    {
        _values = new double[capacity];
        _head = 0;
        _fill = 0;
    }

    internal readonly int Capacity => _values.Length;
    internal readonly bool IsFull => _fill == _values.Length;
    internal readonly bool IsEmpty => _fill == 0;

    internal void Add(double value)
    {
        _values[_head] = value;
        if (++_head >= _values.Length)
        {
            _head = 0;
        }

        if (_fill < _values.Length)
        {
            _fill++;
        }
    }

    internal void Clear()
    {
        _head = 0;
        _fill = 0;
    }

    internal readonly double GetMax()
    {
        if (_fill == 0)
        {
            return double.NaN;
        }

        // the valid entries always occupy slots 0.._fill-1 (all slots once wrapped),
        // so slot order is irrelevant; any NaN in the window propagates
        double max = _values[0];
        if (double.IsNaN(max))
        {
            return double.NaN;
        }

        for (int i = 1; i < _fill; i++)
        {
            double value = _values[i];
            if (double.IsNaN(value))
            {
                return double.NaN;
            }

            if (value > max)
            {
                max = value;
            }
        }

        return max;
    }

    internal readonly double GetMin()
    {
        if (_fill == 0)
        {
            return double.NaN;
        }

        // the valid entries always occupy slots 0.._fill-1 (all slots once wrapped),
        // so slot order is irrelevant; any NaN in the window propagates
        double min = _values[0];
        if (double.IsNaN(min))
        {
            return double.NaN;
        }

        for (int i = 1; i < _fill; i++)
        {
            double value = _values[i];
            if (double.IsNaN(value))
            {
                return double.NaN;
            }

            if (value < min)
            {
                min = value;
            }
        }

        return min;
    }
}
