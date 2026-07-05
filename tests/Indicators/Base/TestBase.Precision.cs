namespace Test.Base;

/// <summary>
/// base for all tests
/// </summary>
public abstract class TestBaseWithPrecision : TestBase
{
    /* MONEY CONSTANTS
     * Precision constants for BeApproximately() assertions for manually calculated values only.
     * Maps to equivalent .Round() precision expectations for Series-style indicators only.
     *
     * IMPORTANT: DO NOT USE this to address floating point precision issues.  */

    // 3 decimal places: ±0.0005 (equivalent to .Round(3))
    protected const double Money2 = 0.005;
    protected const double Money3 = 0.0005;
    protected const double Money4 = 0.00005;
    protected const double Money5 = 0.000005;
    protected const double Money6 = 0.0000005;
    protected const double Money8 = 0.000000005;
    protected const double Money10 = 0.00000000005;
    protected const double Money12 = 0.0000000000005;
}
