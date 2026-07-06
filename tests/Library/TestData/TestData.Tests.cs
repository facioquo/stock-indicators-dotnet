namespace TestOfTests;

[TestClass]
public class TestData : TestBase
{
    // Test the test data to
    // ensure it meets the expected format

    [TestMethod]
    public void BarsIsValid()
    {
        Bars.Should().HaveCount(502);
        Bars.Validate();
    }

    [TestMethod]
    public void OtherBarsIsValid()
    {
        OtherBars.Should().HaveCount(502);
        OtherBars.Validate();
    }

    [TestMethod]
    public void BadBarsIsInvalid()
    {
        BadBars.Should().HaveCount(502);

        // duplicates
        Assert.ThrowsExactly<InvalidBarsException>(
            static () => BadBars.Validate());
    }

    [TestMethod]
    public void BigBarsIsValid()
    {
        BigBars.Should().HaveCount(1246);
        BigBars.Validate();
    }

    [TestMethod]
    public void LongishBarsIsValid()
    {
        LongishBars.Should().HaveCount(5285);
        LongishBars.Validate();
    }

    [TestMethod]
    public void LongestBarsIsValid()
    {
        LongestBars.Should().HaveCount(15821);
        LongestBars.Validate();
    }

    [TestMethod]
    public void MismatchBarsIsValid()
    {
        MismatchBars.Should().HaveCount(502);

        // out of sequence
        Assert.ThrowsExactly<InvalidBarsException>(
            static () => MismatchBars.Validate());
    }

    [TestMethod]
    public void RandomBarsIsValid()
    {
        RandomBars.Should().HaveCount(1000);
        RandomBars.Validate();
    }

    [TestMethod]
    public void ZeroesBarsIsValid()
    {
        ZeroesBars.Should().HaveCount(200);
        ZeroesBars.Validate();
    }

    [TestMethod]
    public void RandomGbmAllowsZeroBarsThenAddProducesData()
    {
        // bars: 0 starts an empty generator to be filled incrementally
        // via Add(DateTime) (used by the offline SSE emulator).
        RandomGbm generator = new(bars: 0);
        generator.Should().BeEmpty();

        DateTime timestamp = new(2020, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        generator.Add(timestamp);

        generator.Should().ContainSingle();
        generator[0].Timestamp.Should().Be(timestamp);

        // a negative count is still rejected
        Assert.ThrowsExactly<ArgumentException>(
            static () => new RandomGbm(bars: -1));
    }
}
