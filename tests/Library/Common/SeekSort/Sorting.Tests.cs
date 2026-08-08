namespace Utilities;

[TestClass]
public class Sorting : TestBase
{
    [TestMethod]
    public void ToSortedList()
    {
        // baseline for comparison
        IReadOnlyList<SmaResult> baseline =
        [
            new(Timestamp: DateTime.Parse("1/1/2000", invariantCulture), Sma: null),
            new(Timestamp: DateTime.Parse("1/2/2000", invariantCulture), Sma: null),
            new(Timestamp: DateTime.Parse("1/9/2000", invariantCulture), Sma: null),
            new(Timestamp: DateTime.Parse("1/3/2000", invariantCulture), Sma: 3),
            new(Timestamp: DateTime.Parse("1/4/2000", invariantCulture), Sma: 4),
            new(Timestamp: DateTime.Parse("1/5/2000", invariantCulture), Sma: 5),
            new(Timestamp: DateTime.Parse("1/6/2000", invariantCulture), Sma: 6),
            new(Timestamp: DateTime.Parse("1/7/2000", invariantCulture), Sma: 7),
            new(Timestamp: DateTime.Parse("1/8/2000", invariantCulture), Sma: double.NaN)
        ];

        // PUBLIC VARIANT, generic sorted list
        IReadOnlyList<SmaResult> sortResults = baseline
            .ToSortedList();

        sortResults[4].Sma.Should().Be(5);
        Assert.AreEqual(DateTime.Parse("1/9/2000", invariantCulture), sortResults[^1].Timestamp);
    }
}
