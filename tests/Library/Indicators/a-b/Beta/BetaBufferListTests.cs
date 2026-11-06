namespace BufferLists;

/// <summary>
/// Series-parity tests for <see cref="BetaList"/>. The pairing rules are covered once,
/// through <see cref="CorrelationList"/>, in <see cref="PairBufferListPairingTests"/>.
/// </summary>
[TestClass]
public class BetaBufferListTests : TestBase
{
    private const int LookbackPeriods = 20;
    private const BetaType Type = BetaType.All;

    // the evaluated side carries SMA warmup NaNs
    private static readonly IReadOnlyList<IReusable> Eval = OtherBars.ToSma(10);
    private static readonly IReadOnlyList<IReusable> Mrkt = Bars;

    [TestMethod]
    public void Factory_MatchesSeriesExactly()
    {
        BetaList list = Eval.ToBetaList(Mrkt, LookbackPeriods, Type);
        list.LookbackPeriods.Should().Be(LookbackPeriods);
        list.Should().HaveCount(Eval.Count);
        list.IsExactly(Eval.ToBeta(Mrkt, LookbackPeriods, Type));
    }

    [TestMethod]
    public void Interleaved_MatchesSeriesExactly()
    {
        BetaList list = new(LookbackPeriods, Type);

        for (int i = 0; i < Eval.Count; i++)
        {
            if (i % 2 == 0)
            {
                list.AddSource(Eval[i]);
                list.AddOther(Mrkt[i]);
            }
            else
            {
                list.AddOther(Mrkt[i]);
                list.AddSource(Eval[i]);
            }
        }

        list.IsExactly(Eval.ToBeta(Mrkt, LookbackPeriods, Type));
    }

    [TestMethod]
    public void EveryType_MatchesSeriesExactly()
    {
        foreach (BetaType type in (BetaType[])[BetaType.Standard, BetaType.Up, BetaType.Down, BetaType.All])
        {
            BetaList list = Eval.ToBetaList(Mrkt, LookbackPeriods, type);

            list.Type.Should().Be(type);
            list.IsExactly(Eval.ToBeta(Mrkt, LookbackPeriods, type), $"{type} must match Series");
        }
    }

    [TestMethod]
    public void ZeroValues_MatchesSeriesExactly()
    {
        // a 0 value on either side makes the next return 0, and a flat market window gives a null Beta
        List<IReusable> evalValues = [.. Eval];
        List<IReusable> mrktValues = [.. Mrkt];
        evalValues[200] = new TimeValue(evalValues[200].Timestamp, 0);
        mrktValues[250] = new TimeValue(mrktValues[250].Timestamp, 0);
        for (int i = 300; i <= 300 + LookbackPeriods; i++)
        {
            mrktValues[i] = new TimeValue(mrktValues[i].Timestamp, 100);
        }

        BetaList list = evalValues.ToBetaList(mrktValues, LookbackPeriods, Type);

        list.IsExactly(evalValues.ToBeta(mrktValues, LookbackPeriods, Type));
        list[201].ReturnsEval.Should().Be(0);
        list[251].ReturnsMrkt.Should().Be(0);
        list[300 + LookbackPeriods].Beta.Should().BeNull();
    }

    [TestMethod]
    public void UnmatchedTimestamps_LookbackSpansMatchedPairs()
    {
        // each side omits a different timestamp inside one lookback window
        List<IReusable> valuesA = [.. Eval];
        List<IReusable> valuesB = [.. Mrkt];
        valuesA.RemoveAt(100);
        valuesB.RemoveAt(110);

        BetaList list = valuesA.ToBetaList(valuesB, LookbackPeriods, Type);

        // the Series method needs both timestamps removed from both sides
        List<IReusable> matchedA = [.. Eval];
        List<IReusable> matchedB = [.. Mrkt];
        matchedA.RemoveAt(110);
        matchedA.RemoveAt(100);
        matchedB.RemoveAt(110);
        matchedB.RemoveAt(100);

        list.Should().HaveCount(Eval.Count - 2);
        list.IsExactly(matchedA.ToBeta(matchedB, LookbackPeriods, Type));
    }

    [TestMethod]
    public void Clear_ResetsLookbackBuffer()
    {
        BetaList list = Eval.ToBetaList(Mrkt, LookbackPeriods, Type);

        list.Clear();
        list.Should().BeEmpty();

        // the returns and lookback window start over too
        list.AddSource(Eval);
        list.AddOther(Mrkt);
        list.IsExactly(Eval.ToBeta(Mrkt, LookbackPeriods, Type));
    }

    [TestMethod]
    public void MaxListSize_PrunesToNewestResults()
    {
        const int maxListSize = 120;

        // added in pairs, so neither side outruns the staging limit
        BetaList list = new(LookbackPeriods, Type) { MaxListSize = maxListSize };

        for (int i = 0; i < Eval.Count; i++)
        {
            list.Add(Eval[i], Mrkt[i]);
        }

        list.Should().HaveCount(maxListSize);
        list.IsExactly(Eval.ToBeta(Mrkt, LookbackPeriods, Type).TakeLast(maxListSize));
    }

    [TestMethod]
    public void Name_IncludesLookback()
        => new BetaList(LookbackPeriods, Type).ToString().Should().Be("BETA(20,ALL)");

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        FluentActions.Invoking(static () => new BetaList(0, Type))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("lookbackPeriods");
        FluentActions.Invoking(static () => Eval.ToBetaList(null!, LookbackPeriods, Type))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceMrkt");
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).ToBetaList(Mrkt, LookbackPeriods, Type))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");

        // a null source is reported before an invalid lookback, as the Series method does
        FluentActions.Invoking(static () => ((IReadOnlyList<IReusable>)null!).ToBetaList(Mrkt, 0))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("sourceEval");
    }
}
