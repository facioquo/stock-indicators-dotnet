namespace FacioQuo.Stock.Indicators;

#pragma warning disable IDE0066 // Convert switch statement to expression
#pragma warning disable IDE0072 // Missing cases in switch statement

/// <summary>
/// Provides numerical utility methods.
/// </summary>
public static class Numerical
{
    /// <summary>
    /// Calculates the standard deviation of an array of double values.
    /// </summary>
    /// <param name="values">Array of double values.</param>
    /// <returns>Standard deviation of the values.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the values array is null.</exception>
    public static double StdDev(this double[] values)
    {
        ArgumentNullException.ThrowIfNull(
            values, "StdDev values cannot be null.");

        int n = values.Length;

        if (n <= 1)
        {
            return 0;
        }

        double sum = 0;

        for (int i = 0; i < n; i++)
        {
            sum += values[i];
        }

        double avg = sum / n;

        double sumSq = 0;
        for (int i = 0; i < n; i++)
        {
            double v = values[i];
            sumSq += (v - avg) * (v - avg);
        }

        return Math.Sqrt(sumSq / n);
    }

    /// <summary>
    /// Calculates the slope of the best fit line for the given x and y values.
    /// </summary>
    /// <param name="x">Array of x values.</param>
    /// <param name="y">Array of y values.</param>
    /// <returns>Slope of the best fit line.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the x or y array is null.</exception>
    /// <exception cref="ArgumentException">Thrown when the x and y arrays are not the same size.</exception>
    public static double Slope(double[] x, double[] y)
    {
        // validate parameters
        ArgumentNullException.ThrowIfNull(x, "Slope X values cannot be null.");
        ArgumentNullException.ThrowIfNull(y, "Slope Y values cannot be null.");

        if (x.Length != y.Length)
        {
            throw new ArgumentException(
                "Slope X and Y arrays must be the same size.");
        }

        int length = x.Length;

        // get averages for period
        double sumX = 0;
        double sumY = 0;

        for (int i = 0; i < length; i++)
        {
            sumX += x[i];
            sumY += y[i];
        }

        double avgX = sumX / length;
        double avgY = sumY / length;

        // least squares method
        double sumSqX = 0;
        double sumSqXy = 0;

        for (int i = 0; i < length; i++)
        {
            double devX = x[i] - avgX;
            double devY = y[i] - avgY;

            sumSqX += devX * devX;
            sumSqXy += devX * devY;
        }

        return sumSqXy / sumSqX;
    }

    /// <summary>
    /// Calculates the natural logarithm of the gamma function.
    /// </summary>
    /// <param name="x">Positive input value.</param>
    /// <returns>Natural logarithm of the gamma function, or NaN for invalid input.</returns>
    internal static double LogGamma(double x)
    {
        if (x <= 0)
        {
            return double.NaN;
        }

        double[] c = [
            0.99999999999980993,
            676.5203681218851,
            -1259.1392167224028,
            771.32342877765313,
            -176.61502916214059,
            12.507343278686905,
            -0.13857109526572012,
            9.9843695780195716e-6,
            1.5056327351493116e-7
        ];

        if (x < 0.5)
        {
            // reflection formula: Gamma(x)Gamma(1-x) = pi/sin(pi*x)
            double sinPiX = Math.Sin(Math.PI * x);
            return sinPiX > 0
                ? DeMath.Log(Math.PI / sinPiX) - LogGamma(1.0 - x)
                : double.NaN;
        }

        x--;
        double a = c[0];
        double t = x + 7.5; // g + 0.5, where g = 7

        for (int i = 1; i <= 8; i++)
        {
            a += c[i] / (x + i);
        }

        return (0.5 * DeMath.Log(2 * Math.PI)) + ((x + 0.5) * DeMath.Log(t)) - t + DeMath.Log(a);
    }

    /// <summary>
    /// Rounds down a DateTime to the nearest interval.
    /// </summary>
    /// <param name="dateTime">DateTime value.</param>
    /// <param name="interval">Interval to round down to.</param>
    /// <returns>Rounded down DateTime value.</returns>
    internal static DateTime RoundDown(
        this DateTime dateTime, TimeSpan interval)
        => interval == TimeSpan.Zero
        ? dateTime
        : dateTime
            .AddTicks(-(dateTime.Ticks % interval.Ticks));

    /// <summary>
    /// Converts a BarInterval to a TimeSpan.
    /// </summary>
    /// <param name="barInterval">BarInterval value.</param>
    /// <returns>Corresponding TimeSpan value.</returns>
    public static TimeSpan ToTimeSpan(this BarInterval barInterval)
        => barInterval switch {
            BarInterval.OneMinute => TimeSpan.FromMinutes(1),
            BarInterval.TwoMinutes => TimeSpan.FromMinutes(2),
            BarInterval.ThreeMinutes => TimeSpan.FromMinutes(3),
            BarInterval.FiveMinutes => TimeSpan.FromMinutes(5),
            BarInterval.FifteenMinutes => TimeSpan.FromMinutes(15),
            BarInterval.ThirtyMinutes => TimeSpan.FromMinutes(30),
            BarInterval.OneHour => TimeSpan.FromHours(1),
            BarInterval.TwoHours => TimeSpan.FromHours(2),
            BarInterval.FourHours => TimeSpan.FromHours(4),
            BarInterval.Day => TimeSpan.FromDays(1),
            BarInterval.Week => TimeSpan.FromDays(7),
            // intentionally skipping Month
            _ => TimeSpan.Zero
        };

    /// <summary>
    /// Determines the number of decimal places in a decimal value.
    /// </summary>
    /// <param name="n">Decimal value.</param>
    /// <returns>Number of decimal places.</returns>
    internal static int GetDecimalPlaces(this decimal n)
    {
        // source: https://stackoverflow.com/a/30205131/4496145

        n = Math.Abs(n); // make sure it is positive.
        n -= (int)n;     // remove the integer part of the number.
        int decimalPlaces = 0;
        while (n > 0)
        {
            decimalPlaces++;
            n *= 10;
            n -= (int)n;
        }

        return decimalPlaces;
    }

    /// <summary>
    /// Determines if a type is a numeric non-date type.
    /// </summary>
    /// <param name="type">Data <see cref="Type"/></param>
    /// <returns>True if numeric type.</returns>
    internal static bool IsNumeric(this Type type)
    {

        if (type == typeof(DateTime) ||
            type == typeof(DateTimeOffset) ||
            type == typeof(DateOnly))
        {
            return false;
        }

        Type realType = Nullable.GetUnderlyingType(type) ?? type;

        switch (Type.GetTypeCode(realType))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.Int16:
            case TypeCode.UInt16:
            case TypeCode.Int32:
            case TypeCode.UInt32:
            case TypeCode.Int64:
            case TypeCode.UInt64:
            case TypeCode.Single:
            case TypeCode.Double:
            case TypeCode.Decimal:
                return true;
            default:
                return false;
        }
    }
}
