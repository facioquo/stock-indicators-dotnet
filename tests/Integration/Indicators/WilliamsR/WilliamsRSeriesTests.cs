using Test.Tools;
using Tests.Data;

namespace StaticSeries;

[TestClass, TestCategory("Integration")]
public class WilliamsRSeriesTests
{
    [TestMethod]
    public async Task ToWilliamsR_LiveData_MaintainsBoundaryRange()
    {
        // initialize
        IEnumerable<Bar> feedBars = await FeedData  // live bars
            .GetBars("A", 365 * 3)
            .ConfigureAwait(false);

        List<Bar> bars = feedBars.ToList();

        // get indicators
        IReadOnlyList<WilliamsResult> results = bars
            .ToWilliamsR(14);

        results.ToConsole(static r => r.WilliamsR is > 0d or < -100d, (nameof(WilliamsResult.WilliamsR), "F20"));

        results.IsBetween(static results => results.WilliamsR, -100d, 0d);
    }
}
