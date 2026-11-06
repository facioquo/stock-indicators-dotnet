namespace BufferLists;

/// <summary>
/// Pairing rules shared by every <see cref="PairBufferList{TResult}"/>, tested once through
/// <see cref="CorrelationList"/>: the list must equal the Series method over the timestamps
/// both sides hold, whatever order the sides arrive in.
/// </summary>
[TestClass]
public class PairBufferListPairingTests : TestBase
{
    // a short lookback, so a window restarted by pruning or Clear still yields values
    private const int LookbackPeriods = 5;

    // warmup NaNs on both sides
    private static readonly IReadOnlyList<IReusable> Source = Bars.ToSma(10);
    private static readonly IReadOnlyList<IReusable> Other = OtherBars.ToEma(20);

    private static readonly IReadOnlyList<CorrResult> Expected
        = Source.ToCorrelation(Other, LookbackPeriods);

    [TestMethod]
    public void AddSourceThenOther_MatchesSeriesExactly()
    {
        CorrelationList list = new(LookbackPeriods);
        list.AddSource(Source);
        list.Should().BeEmpty();
        list.AddOther(Other);

        list.IsExactly(Expected);
    }

    [TestMethod]
    public void AddOtherThenSource_MatchesSeriesExactly()
    {
        CorrelationList list = new(LookbackPeriods);
        list.AddOther(Other);
        list.Should().BeEmpty();
        list.AddSource(Source);

        list.IsExactly(Expected);
    }

    [TestMethod]
    public void InterleavedTimestampValues_MatchesSeriesExactly()
    {
        CorrelationList list = new(LookbackPeriods);

        for (int i = 0; i < Source.Count; i++)
        {
            if (i % 2 == 0)
            {
                list.AddSource(Source[i].Timestamp, Source[i].Value);
                list.AddOther(Other[i].Timestamp, Other[i].Value);
            }
            else
            {
                list.AddOther(Other[i].Timestamp, Other[i].Value);
                list.AddSource(Source[i].Timestamp, Source[i].Value);
            }
        }

        list.IsExactly(Expected);
    }

    [TestMethod]
    public void PairedAdd_MatchesSeriesExactly()
    {
        CorrelationList list = new(LookbackPeriods);

        for (int i = 0; i < Source.Count; i++)
        {
            list.Add(Source[i], Other[i]);
        }

        list.IsExactly(Expected);
    }

    [TestMethod]
    public void OneSideLags_ResultsArriveWithTheLaggingSide()
    {
        CorrelationList list = new(LookbackPeriods);
        list.AddSource(Source.Take(100).ToList());

        for (int i = 0; i < 100; i++)
        {
            list.AddOther(Other[i]);
            list.Should().HaveCount(i + 1);
        }

        list.IsExactly(Expected.Take(100));
    }

    [TestMethod]
    public void UnmatchedTimestamp_ProducesNoResult()
    {
        const int skip = 50;
        List<IReusable> otherWithGap = [.. Other];
        otherWithGap.RemoveAt(skip);

        CorrelationList list = new(LookbackPeriods, Source, otherWithGap);

        list.Should().HaveCount(Source.Count - 1);
        list.Should().NotContain(x => x.Timestamp == Source[skip].Timestamp);

        List<IReusable> sourceWithGap = [.. Source];
        sourceWithGap.RemoveAt(skip);
        list.IsExactly(sourceWithGap.ToCorrelation(otherWithGap, LookbackPeriods));
    }

    [TestMethod]
    public void OffsetRanges_SkipUnmatchedWhereSeriesThrows()
    {
        // the Series method rejects these inputs; the list pairs the overlap
        IReadOnlyList<IReusable> late = Source.Skip(100).ToList();
        IReadOnlyList<IReusable> early = Other.Take(400).ToList();

        FluentActions.Invoking(() => late.ToCorrelation(early, LookbackPeriods))
            .Should().Throw<Exception>();

        CorrelationList overlap = new(LookbackPeriods, late, early);
        overlap.Should().HaveCount(300);
        overlap.IsExactly(
            Source.Skip(100).Take(300).ToList()
                .ToCorrelation(Other.Skip(100).Take(300).ToList(), LookbackPeriods));

        CorrelationList disjoint = new(LookbackPeriods, Source.Skip(300).ToList(), Other.Take(300).ToList());
        disjoint.Should().BeEmpty();
    }

    [TestMethod]
    public void OutOfOrderOnOneSide_Throws()
    {
        CorrelationList list = new(LookbackPeriods, Source.Take(10).ToList(), Other.Take(10).ToList());

        // the check is not opt-in, unlike the inherited one
        list.ThrowOnOutOfOrder.Should().BeFalse();

        FluentActions.Invoking(() => list.AddSource(Source[5]))
            .Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*source timestamp*not after the last source timestamp*");

        FluentActions.Invoking(() => list.AddOther(Other[9]))
            .Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*other timestamp*not after the last other timestamp*");

        // an equal timestamp is a revision, which a BufferList cannot apply
        FluentActions.Invoking(() => list.AddSource(Source[9]))
            .Should().ThrowExactly<InvalidOperationException>();
        FluentActions.Invoking(() => list.AddOther(Other[9].Timestamp, Other[9].Value))
            .Should().ThrowExactly<InvalidOperationException>();

        list.Should().HaveCount(10);

        // a value one side skipped cannot be added late; that needs a StreamHub
        CorrelationList gapped = new(LookbackPeriods);
        gapped.AddSource(Source[0]);
        gapped.AddOther(Other[1]);

        FluentActions.Invoking(() => gapped.AddOther(Other[0]))
            .Should().ThrowExactly<InvalidOperationException>();
    }

    [TestMethod]
    public void Clear_ResetsListAndStagedValues()
    {
        CorrelationList list = new(LookbackPeriods);
        list.AddSource(Source);
        list.AddOther(Other.Take(100).ToList());
        list.Should().HaveCount(100);

        list.Clear();
        list.Should().BeEmpty();

        // the staged source values are gone too, and ordering starts over
        list.AddOther(Other);
        list.Should().BeEmpty();
        list.AddSource(Source);
        list.IsExactly(Expected);
    }

    [TestMethod]
    public void MaxListSize_PrunesToNewestResults()
    {
        const int maxListSize = 120;

        // paired adds keep nothing staged, so only the result list is pruned
        CorrelationList list = new(LookbackPeriods) { MaxListSize = maxListSize };
        for (int i = 0; i < Source.Count; i++)
        {
            list.Add(Source[i], Other[i]);
        }

        list.Should().HaveCount(maxListSize);
        list.IsExactly(Expected.TakeLast(maxListSize));
    }

    [TestMethod]
    public void PairedAdd_OutOfOrderOther_LeavesListUnchanged()
    {
        CorrelationList list = new(LookbackPeriods);
        list.Add(Source[0], Other[0]);
        list.Add(Source[1], Other[1]);

        FluentActions.Invoking(() => list.Add(Source[2], Other[1]))
            .Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*other timestamp*");

        // the source side was not staged, so the corrected pair is accepted
        list.Should().HaveCount(2);
        list.Add(Source[2], Other[2]);
        list.Should().HaveCount(3);
        list.IsExactly(Expected.Take(3));
    }

    [TestMethod]
    public void MaxListSize_LaggingSideStopsPartway_LosesDroppedPairs()
    {
        // the source runs 100 values ahead, so only its newest 50 stay staged;
        // the other side then supplies 60, of which the first 50 find nothing to pair
        CorrelationList list = new(LookbackPeriods) { MaxListSize = 50 };
        list.AddSource(Source.Take(100).ToList());
        list.AddOther(Other.Take(60).ToList());

        list.Should().HaveCount(10);
        list.IsExactly(
            Source.Skip(50).Take(10).ToList()
                .ToCorrelation(Other.Skip(50).Take(10).ToList(), LookbackPeriods));
    }

    [TestMethod]
    public void MaxListSize_LoweredAfterStaging_AppliesOnNextAdd()
    {
        CorrelationList list = new(LookbackPeriods);
        list.AddSource(Source.Take(100).ToList());

        // the smaller limit trims the source side as soon as it is added to again
        list.MaxListSize = 20;
        list.AddSource(Source.Skip(100).Take(10).ToList());
        list.AddOther(Other.Take(60).ToList());

        // only source timestamps 90 to 109 stay staged, so none of the 60 find a partner;
        // without the shrink all 60 would pair and the result list would hold 20
        list.Should().BeEmpty();

        list.AddOther(Other.Skip(60).Take(50).ToList());
        list.Should().HaveCount(20);
        list.IsExactly(
            Source.Skip(90).Take(20).ToList()
                .ToCorrelation(Other.Skip(90).Take(20).ToList(), LookbackPeriods));
    }

    [TestMethod]
    public void NullArguments_Throw()
    {
        CorrelationList list = new(LookbackPeriods);

        FluentActions.Invoking(() => list.AddSource((IReusable)null!))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => list.AddOther((IReusable)null!))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => list.AddSource((IReadOnlyList<IReusable>)null!))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => list.AddOther((IReadOnlyList<IReusable>)null!))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => list.Add(null!, Other[0]))
            .Should().ThrowExactly<ArgumentNullException>();
        FluentActions.Invoking(() => list.Add(Source[0], null!))
            .Should().ThrowExactly<ArgumentNullException>();

        // a null element in a batch is reported, not dereferenced
        List<IReusable> withNull = [Source[0], null!];
        FluentActions.Invoking(() => list.AddSource(withNull))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("values");
        FluentActions.Invoking(() => new CorrelationList(LookbackPeriods, withNull, Other))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("source");
        FluentActions.Invoking(() => new CorrelationList(LookbackPeriods, Source, withNull))
            .Should().ThrowExactly<ArgumentNullException>().WithParameterName("other");
    }
}
