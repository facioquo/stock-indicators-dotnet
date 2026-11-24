namespace BufferLists;

/// <summary>
/// Series-parity tests for <see cref="PrsList"/>. The pairing rules are covered once,
/// through <see cref="CorrelationList"/>, in <see cref="PairBufferListPairingTests"/>.
/// </summary>
[TestClass]
public class PrsBufferListTests : TestBase
{
    private const int LookbackPeriods = 20;

    // the evaluated side carries SMA warmup NaNs
    private static readonly IReadOnlyList<IReusable> Eval = OtherBars.ToSma(10);
    private static readonly IReadOnlyList<IReusable> Base = Bars;

    [TestMethod]
    public void Factories_MatchSeriesExactly()
    {
        PrsList list = Eval.ToPrsList(Base, LookbackPeriods);
        list.LookbackPeriods.Should().Be(LookbackPeriods);
        list.Should().HaveCount(Eval.Count);
        list.IsExactly(Eval.ToPrs(Base, LookbackPeriods));

        PrsList noPercent = Eval.ToPrsList(Base);
        noPercent.LookbackPeriods.Should().Be(0);
        noPercent.IsExactly(Eval.ToPrs(Base));
        noPercent.Should().OnlyContain(static x => x.PrsPercent == null);
    }

    [TestMethod]
    public void Interleaved_MatchesSeriesExactly()
    {
        PrsList list = [with(LookbackPeriods)];

        for (int i = 0; i < Eval.Count; i++)
        {
            if (i % 2 == 0)
            {
                list.AddSource(Eval[i]);
                list.AddOther(Base[i]);
            }
            else
            {
                list.AddOther(Base[i]);
                list.AddSource(Eval[i]);
            }
        }

        list.IsExactly(Eval.ToPrs(Base, LookbackPeriods));
    }

    [TestMethod]
    public void UnmatchedTimestamps_LookbackSpansMatchedPairs()
    {
        // each side omits a different timestamp inside one lookback window
        List<IReusable> evalValues = [.. Eval];
        List<IReusable> baseValues = [.. Base];
        evalValues.RemoveAt(100);
        baseValues.RemoveAt(110);

        PrsList list = evalValues.ToPrsList(baseValues, LookbackPeriods);

        // the Series method needs both timestamps removed from both sides
        List<IReusable> evalMatched = [.. Eval];
        List<IReusable> baseMatched = [.. Base];
        evalMatched.RemoveAt(110);
        evalMatched.RemoveAt(100);
        baseMatched.RemoveAt(110);
        baseMatched.RemoveAt(100);

        list.Should().HaveCount(Eval.Count - 2);
        list.IsExactly(evalMatched.ToPrs(baseMatched, LookbackPeriods));
    }

    [TestMethod]
    public void ZeroValues_MatchesSeriesExactly()
    {
        // a 0 base gives a null PRS; a 0 earlier value on either side gives a null PRS%
        List<IReusable> evalValues = [.. Eval];
        List<IReusable> baseValues = [.. Base];
        evalValues[200] = new TimeValue(evalValues[200].Timestamp, 0);
        baseValues[300] = new TimeValue(baseValues[300].Timestamp, 0);

        PrsList list = evalValues.ToPrsList(baseValues, LookbackPeriods);

        list.IsExactly(evalValues.ToPrs(baseValues, LookbackPeriods));
        list[300].Prs.Should().BeNull();
        list[200 + LookbackPeriods].PrsPercent.Should().BeNull();
        list[300 + LookbackPeriods].PrsPercent.Should().BeNull();
    }

    [TestMethod]
    public void Clear_ResetsLookbackBuffer()
    {
        PrsList list = Eval.ToPrsList(Base, LookbackPeriods);

        list.Clear();
        list.Should().BeEmpty();

        // the PRS% window starts over too
        list.AddSource(Eval);
        list.AddOther(Base);
        list.IsExactly(Eval.ToPrs(Base, LookbackPeriods));
    }

    [TestMethod]
    public void MaxListSize_PrunesToNewestResults()
    {
        const int maxListSize = 120;

        // added in pairs, so neither side outruns the staging limit
        PrsList list = new(LookbackPeriods) { MaxListSize = maxListSize };

        for (int i = 0; i < Eval.Count; i++)
        {
            list.Add(Eval[i], Base[i]);
        }

        list.Should().HaveCount(maxListSize);
        list.IsExactly(Eval.ToPrs(Base, LookbackPeriods).TakeLast(maxListSize));
    }

    [TestMethod]
    public void Name_IncludesLookback()
    {
        new PrsList(LookbackPeriods).Name.Should().Be("PRS(20)");
        new PrsList().ToString().Should().Be("PRS");
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        FluentActions.Invoking(static () => new PrsList(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(static () => Eval.ToPrsList(null!, LookbackPeriods))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceBase");
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).ToPrsList(Base))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");

        // a null source is reported before an invalid lookback, as the Series method does
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).ToPrsList(Base, 0))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");
    }
}
