namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Fixed-size circular buffer for <c>double</c> values with O(capacity) max/min scan.
/// Zero heap allocation per tick; cache-line-resident for small windows (&lt;= ~32 elements).
/// </summary>
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

        double max = _values[0];
        for (int i = 1; i < _fill; i++)
        {
            if (_values[i] > max)
            {
                max = _values[i];
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

        double min = _values[0];
        for (int i = 1; i < _fill; i++)
        {
            if (_values[i] < min)
            {
                min = _values[i];
            }
        }

        return min;
    }
}
