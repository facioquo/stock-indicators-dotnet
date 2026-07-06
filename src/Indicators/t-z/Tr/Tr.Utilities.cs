namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides utility methods for calculating True Range (TR).
/// </summary>
public static partial class Tr
{
    /// <summary>
    /// Calculates the True Range increment.
    /// </summary>
    /// <param name="high">High price.</param>
    /// <param name="low">Low price.</param>
    /// <param name="prevClose">Previous close price.</param>
    /// <returns>True Range increment.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Increment(
        double high,
        double low,
        double prevClose)
    {
        double hmpc = Math.Abs(high - prevClose);
        double lmpc = Math.Abs(low - prevClose);

        return Math.Max(high - low, Math.Max(hmpc, lmpc));
    }
}
