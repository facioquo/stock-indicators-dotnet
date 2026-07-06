namespace StaticSeries;

public partial class SmaSeriesTests : StaticSeriesTestBase
{
    [TestMethod]
    public void Average_OverTimeValueArray_ReturnsExpectedResult()
    {
        TimeValue[] results =
        [
            new TimeValue(Bars[0].Timestamp, 0.0),
            new TimeValue(Bars[1].Timestamp, 4.0),
            new TimeValue(Bars[2].Timestamp, 8.0)
        ];

        // calculate
        double? mid = results.Average(2, 1);
        double? end = results.Average(2);

        // assert
        mid.Should().Be(2);
        end.Should().Be(6);
    }
}
