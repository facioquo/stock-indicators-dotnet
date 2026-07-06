namespace Test.Base;

/// <summary>
/// Base tests that all series indicators should have.
/// </summary>
public abstract class StaticSeriesTestBase : TestBaseWithPrecision
{
    /// <summary>
    /// Tests default use case and parameters arguments
    /// </summary>
    public abstract void DefaultParameters_ReturnsExpectedResults();

    /// <summary>
    /// Tests proper handling of incompatible bar data
    /// </summary>
    public abstract void BadBars_DoesNotFail();

    /// <summary>
    /// Tests that empty bars sets return empty results set
    /// </summary>
    public abstract void NoBars_ReturnsEmpty();
}
