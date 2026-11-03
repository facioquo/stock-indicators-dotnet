namespace BufferLists;

/// <summary>
/// Series-parity tests for <see cref="CorrelationList"/>. The pairing rules are covered once,
/// through <see cref="PairMathList"/>, in <see cref="PairMathListTests"/>.
/// </summary>
[TestClass]
public class CorrelationBufferListTests : TestBase
{
    private const int LookbackPeriods = 20;

    // series A carries SMA warmup NaNs
    private static readonly IReadOnlyList<IReusable> SourceA = OtherBars.ToSma(10);
    private static readonly IReadOnlyList<IReusable> SourceB = Bars;

    [TestMethod]
    public void Factory_MatchesSeriesExactly()
    {
        CorrelationList list = SourceA.ToCorrelationList(SourceB, LookbackPeriods);
        list.LookbackPeriods.Should().Be(LookbackPeriods);
        list.Should().HaveCount(SourceA.Count);
        list.IsExactly(SourceA.ToCorrelation(SourceB, LookbackPeriods));
    }

    [TestMethod]
    public void Interleaved_MatchesSeriesExactly()
    {
        CorrelationList list = new(LookbackPeriods);

        for (int i = 0; i < SourceA.Count; i++)
        {
            if (i % 2 == 0)
            {
                list.AddSource(SourceA[i]);
                list.AddOther(SourceB[i]);
            }
            else
            {
                list.AddOther(SourceB[i]);
                list.AddSource(SourceA[i]);
            }
        }

        list.IsExactly(SourceA.ToCorrelation(SourceB, LookbackPeriods));
    }

    [TestMethod]
    public void FlatWindow_GivesNullCorrelation_MatchesSeriesExactly()
    {
        // a flat run on series B zeroes its variance, so the divisor is 0
        List<IReusable> valuesB = [.. SourceB];
        for (int i = 200; i < 200 + LookbackPeriods; i++)
        {
            valuesB[i] = new TimeValue(valuesB[i].Timestamp, 100);
        }

        CorrelationList list = SourceA.ToCorrelationList(valuesB, LookbackPeriods);

        list.IsExactly(SourceA.ToCorrelation(valuesB, LookbackPeriods));
        list[200 + LookbackPeriods - 1].Correlation.Should().BeNull();
    }

    [TestMethod]
    public void Clear_ResetsLookbackBuffer()
    {
        CorrelationList list = SourceA.ToCorrelationList(SourceB, LookbackPeriods);

        list.Clear();
        list.Should().BeEmpty();

        // the lookback window starts over too
        list.AddSource(SourceA);
        list.AddOther(SourceB);
        list.IsExactly(SourceA.ToCorrelation(SourceB, LookbackPeriods));
    }

    [TestMethod]
    public void MaxListSize_PrunesToNewestResults()
    {
        const int maxListSize = 120;

        // added in pairs, so neither side outruns the staging limit
        CorrelationList list = new(LookbackPeriods) { MaxListSize = maxListSize };

        for (int i = 0; i < SourceA.Count; i++)
        {
            list.Add(SourceA[i], SourceB[i]);
        }

        list.Should().HaveCount(maxListSize);
        list.IsExactly(SourceA.ToCorrelation(SourceB, LookbackPeriods).TakeLast(maxListSize));
    }

    [TestMethod]
    public void Name_IncludesLookback()
        => new CorrelationList(LookbackPeriods).ToString().Should().Be("CORR(20)");

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        FluentActions.Invoking(static () => new CorrelationList(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(static () => SourceA.ToCorrelationList(null!, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceB");
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).ToCorrelationList(SourceB, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceA");
    }
}
