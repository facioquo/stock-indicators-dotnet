namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Deterministic math helpers to remove platform-dependent rounding drift.
/// </summary>
internal static class DeMath
{
    private const double Ln2 = 0.693147180559945309417232121458176568;
    private const double InvLn2 = 1.442695040888963407359924681001892137;

    // ln(2) split into high (~32 leading bits exact) and low residual parts,
    // used to reduce cancellation error when computing r = x - k*ln(2).
    private const double Ln2Hi = 6.93147180369123816490e-01;
    private const double Ln2Lo = 1.90821492927058770002e-10;

    // Overflow and underflow thresholds for Exp on IEEE 754 doubles.
    private const double ExpOverflow = 709.782712893383973096d;   // ln(double.MaxValue)
    private const double ExpUnderflow = -745.133219101941108420d; // ln(double.Epsilon)

    private const double InvLn10 = 0.434294481903251827651128918916605082;
    private const double TwoPow52 = 4503599627370496d; // 2^52
    private const double TwoPow54 = 18014398509481984d; // 2^54
    private const double TwoPowNeg54 = 5.551115123125783e-17; // 2^-54
    private const double HalfPi = Math.PI / 2d;
    private const double Pi = Math.PI;

    private static readonly double[] AtanTable =
    [
        0.78539816339744827900,
        0.46364760900080609352,
        0.24497866312686414347,
        0.12435499454676143816,
        0.06241880999595735002,
        0.03123983343026827625,
        0.01562372862047683129,
        0.00781234106010111114,
        0.00390623013196697183,
        0.00195312251647881876,
        0.00097656218955931943,
        0.00048828121119489829,
        0.00024414062014936177,
        0.00012207031189367021,
        0.00006103515617420877,
        0.00003051757811552610,
        0.00001525878906131576,
        0.00000762939453110197,
        0.00000381469726560650,
        0.00000190734863281019,
        0.00000095367431640596,
        0.00000047683715820309,
        0.00000023841857910156,
        0.00000011920928955078,
        0.00000005960464477539,
        0.00000002980232238770,
        0.00000001490116119385,
        0.00000000745058059692,
        0.00000000372529029846,
        0.00000000186264514923,
        0.00000000093132257462,
        0.00000000046566128731
    ];

    /// <summary>
    /// Deterministic natural log implementation that avoids
    /// platform-specific drift.
    /// </summary>
    /// <remarks>
    /// using mantissa/exponent extraction is deterministic and generally well-behaved
    /// because 𝑦=(𝑚−1)/(𝑚+1) stays small.
    /// </remarks>
    /// <param name="x">Input value.</param>
    /// <returns>Natural log of <paramref name="x"/>.</returns>
    internal static double Log(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (x < 0d)
        {
            return double.NaN;
        }

        if (x == 0d)
        {
            return double.NegativeInfinity;
        }

        if (double.IsPositiveInfinity(x))
        {
            return double.PositiveInfinity;
        }

        // Extract exponent and mantissa in a platform-neutral way
        long bits = BitConverter.DoubleToInt64Bits(x);
        int exponent = (int)((bits >> 52) & 0x7FF);

        if (exponent == 0)
        {
            x *= TwoPow52;
            bits = BitConverter.DoubleToInt64Bits(x);
            exponent = (int)((bits >> 52) & 0x7FF) - 52;
        }

        exponent -= 1023;
        bits = (bits & 0x000F_FFFF_FFFF_FFFFL) | 0x3FF0_0000_0000_0000L;
        double mantissa = BitConverter.Int64BitsToDouble(bits);

        // log via atanh series: ln(m) = 2 * [ y + y^3/3 + y^5/5 + ... ]
        double y = (mantissa - 1d) / (mantissa + 1d);
        double y2 = y * y;

        double series = y;
        double term = y;

        for (int k = 1; k < 20; k++)
        {
            term *= y2;
            int denominator = (k << 1) + 1;
            series += term / denominator;
        }

        double lnMantissa = series + series; // multiply by 2 once to reduce rounding
        double exponentPart = exponent * Ln2;

        return exponentPart + lnMantissa;
    }

    /// <summary>
    /// Deterministic base-10 log implementation built on <see cref="Log(double)"/>.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <returns>Base-10 log of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Log10(double x)
        => Log(x) * InvLn10;

    /// <summary>
    /// Deterministic natural exponential implementation that avoids
    /// platform-specific drift.
    /// </summary>
    /// <remarks>
    /// Range reduction: exp(x) = 2^k * exp(r), where k = round(x / ln(2))
    /// and r = x - k*ln(2) with |r| &lt;= ln(2)/2 ≈ 0.347. The reduced
    /// value is evaluated by a Taylor series; the 2^k scale is applied
    /// by direct exponent-bit manipulation. ln(2) is carried as a hi/lo
    /// split to suppress cancellation when subtracting k*ln(2) from x.
    /// </remarks>
    /// <param name="x">Input value.</param>
    /// <returns>e raised to the power of <paramref name="x"/>.</returns>
    internal static double Exp(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (x >= ExpOverflow)
        {
            return double.PositiveInfinity;
        }

        if (x <= ExpUnderflow)
        {
            return 0d;
        }

        if (x == 0d)
        {
            return 1d;
        }

        // Range reduction: choose integer k closest to x / ln(2),
        // using round-half-away-from-zero (deterministic across platforms).
        double kReal = x * InvLn2;
        long k = (long)(kReal >= 0d ? kReal + 0.5d : kReal - 0.5d);

        // Compute r = x - k*ln(2) using the hi/lo split of ln(2).
        double kDouble = k;
        double r = x - (kDouble * Ln2Hi) - (kDouble * Ln2Lo);

        // Taylor series for exp(r): sum_{n>=0} r^n / n!
        // With |r| <= ln(2)/2, 17 terms give well under 1 ulp error.
        double term = 1d;
        double sum = 1d;

        for (int n = 1; n <= 17; n++)
        {
            term *= r / n;
            sum += term;
        }

        // Multiply by 2^k. For k in [-1022, 1023] the result is normal
        // and 2^k can be built by setting the biased exponent directly.
        // Outside that range, split the scale so each factor remains
        // representable as a normal double.
        if (k is >= -1022L and <= 1023L)
        {
            long expBits = (k + 1023L) << 52;
            return sum * BitConverter.Int64BitsToDouble(expBits);
        }

        if (k > 1023L)
        {
            // High-normal split: 2^k = 2^(k-54) * 2^54.
            // Reachable near the overflow boundary (e.g. x close to ln(double.MaxValue))
            // where round-to-nearest produces k = 1024 but exp(x) is still finite.
            long highBits = (k - 54L + 1023L) << 52;
            double pow2Lo = BitConverter.Int64BitsToDouble(highBits);
            return sum * pow2Lo * TwoPow54;
        }

        // Subnormal split: 2^k = 2^(k+54) * 2^(-54).
        long subBits = (k + 54L + 1023L) << 52;
        double pow2Hi = BitConverter.Int64BitsToDouble(subBits);
        return sum * pow2Hi * TwoPowNeg54;
    }

    /// <summary>
    /// Deterministic inverse hyperbolic tangent.
    /// </summary>
    /// <param name="x">Input value in (-1, 1).</param>
    /// <returns>atanh of <paramref name="x"/>.</returns>
    internal static double Atanh(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (x >= 1d)
        {
            return x == 1d
                ? double.PositiveInfinity
                : double.NaN;
        }

        if (x <= -1d)
        {
            return x == -1d
                ? double.NegativeInfinity
                : double.NaN;
        }

        // Use log-difference form to avoid an intermediate division overflow:
        // atanh(x) = 0.5 * (ln(1+x) - ln(1-x))
        if (x == 0d)
        {
            return 0d;
        }

        double ln1px = Log(1d + x);
        double ln1mx = Log(1d - x);

        return 0.5d * (ln1px - ln1mx);
    }

    /// <summary>
    /// Deterministic arctangent using a fixed CORDIC table.
    /// </summary>
    /// <param name="x">Input value.</param>
    /// <returns>atan of <paramref name="x"/>.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Atan(double x)
    {
        if (double.IsNaN(x))
        {
            return double.NaN;
        }

        if (double.IsPositiveInfinity(x))
        {
            return HalfPi;
        }

        if (double.IsNegativeInfinity(x))
        {
            return -HalfPi;
        }

        if (x == 0d)
        {
            return 0d;
        }

        // Range reduction: atan(x) = sign(x) * (pi/2 - atan(1/|x|)) for |x| > 1
        // This prevents overflow in the CORDIC vectoring step and improves accuracy for large |x|.
        double ax = x < 0d ? -x : x;
        if (ax > 1d)
        {
            double reduced = AtanCordic(1d / ax);
            double r = HalfPi - reduced;
            return x < 0d ? -r : r;
        }

        // |x| <= 1: direct CORDIC
        double a = AtanCordic(ax);
        return x < 0d ? -a : a;
    }

    /// <summary>
    /// Deterministic atan2 built from deterministic atan.
    /// Matches Math.Atan2 quadrant conventions.
    /// </summary>
    /// <param name="y">Y coordinate value</param>
    /// <param name="x">X coordinate value</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static double Atan2(double y, double x)
    {
        if (double.IsNaN(x) || double.IsNaN(y))
        {
            return double.NaN;
        }

        if (x > 0d)
        {
            return Atan(y / x);
        }

        if (x < 0d)
        {
            double a = Atan(y / x);

            // Preserve signed-zero semantics: for y == +0.0 -> +pi, for y == -0.0 -> -pi
            if (y > 0d)
            {
                return a + Pi;
            }

            if (y < 0d)
            {
                return a - Pi;
            }

            // y == 0.0: examine sign bit to distinguish +0.0 vs -0.0
            long yBits = BitConverter.DoubleToInt64Bits(y);
            bool yIsPositiveZero = yBits >= 0L;
            return yIsPositiveZero ? a + Pi : a - Pi;
        }

        // x == 0
        if (y > 0d)
        {
            return HalfPi;
        }

        if (y < 0d)
        {
            return -HalfPi;
        }

        // Preserve signed-zero for (0,0) by returning `y` (will be +0.0 or -0.0)
        return y;
    }

    // CORDIC core assumes input is finite and |x| <= 1.
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static double AtanCordic(double x)
    {
        // CORDIC vectoring with constant x=1.0 to get atan(y)
        double currentX = 1d;
        double currentY = x;
        double angle = 0d;
        double factor = 1d;

        for (int i = 0; i < AtanTable.Length; i++)
        {
            double a = AtanTable[i];
            if (currentY >= 0d)
            {
                double nextX = currentX + (currentY * factor);
                double nextY = currentY - (currentX * factor);
                angle += a;

                currentX = nextX;
                currentY = nextY;
            }
            else
            {
                double nextX = currentX - (currentY * factor);
                double nextY = currentY + (currentX * factor);
                angle -= a;

                currentX = nextX;
                currentY = nextY;
            }

            factor *= 0.5d;
        }

        return angle;
    }
}
