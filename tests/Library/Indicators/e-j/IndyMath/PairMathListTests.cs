namespace BufferLists;

/// <summary>
/// Series-parity tests for <see cref="PairMathList"/>: the list must equal the Series
/// method over the timestamps both sides hold, whatever order the sides arrive in.
/// </summary>
[TestClass]
public class PairMathListTests : TestBase
{
    private static readonly (PairOperation Operation, Func<IReadOnlyList<IReusable>, IReadOnlyList<IReusable>, IReadOnlyList<MathResult>> Series, Func<IReadOnlyList<IReusable>, IReadOnlyList<IReusable>, PairMathList> Factory)[] Operations = [
        (PairOperation.Plus, static (s, o) => s.Plus(o), static (s, o) => s.ToPlusList(o)),
        (PairOperation.Minus, static (s, o) => s.Minus(o), static (s, o) => s.ToMinusList(o)),
        (PairOperation.Times, static (s, o) => s.Times(o), static (s, o) => s.ToTimesList(o)),
        (PairOperation.DividedBy, static (s, o) => s.DividedBy(o), static (s, o) => s.ToDividedByList(o)),
        (PairOperation.MaxWith, static (s, o) => s.MaxWith(o), static (s, o) => s.ToMaxWithList(o)),
        (PairOperation.MinWith, static (s, o) => s.MinWith(o), static (s, o) => s.ToMinWithList(o))
    ];

    // warmup NaNs on both sides, plus a 0 divisor that DividedBy must turn into null
    private static readonly IReadOnlyList<IReusable> Source = Bars.ToSma(10);
    private static readonly IReadOnlyList<IReusable> Other = WithZero(OtherBars.ToEma(20), 300);

    [TestMethod]
    public void Factories_MatchSeriesExactly()
    {
        foreach ((PairOperation operation, Func<IReadOnlyList<IReusable>, IReadOnlyList<IReusable>, IReadOnlyList<MathResult>> series, Func<IReadOnlyList<IReusable>, IReadOnlyList<IReusable>, PairMathList> factory) in Operations)
        {
            PairMathList list = factory(Source, Other);

            list.Operation.Should().Be(operation);
            list.Should().HaveCount(Source.Count);
            list.IsExactly(series(Source, Other), $"{operation} must match Series");
        }
    }

    [TestMethod]
    public void AddSourceThenOther_MatchesSeriesExactly()
        => AssertEveryOperation(static list => {
            list.AddSource(Source);
            list.Should().BeEmpty();
            list.AddOther(Other);
        });

    [TestMethod]
    public void AddOtherThenSource_MatchesSeriesExactly()
        => AssertEveryOperation(static list => {
            list.AddOther(Other);
            list.Should().BeEmpty();
            list.AddSource(Source);
        });

    [TestMethod]
    public void Interleaved_MatchesSeriesExactly()
        => AssertEveryOperation(static list => {
            for (int i = 0; i < Source.Count; i++)
            {
                if (i % 2 == 0)
                {
                    list.AddSource(Source[i]);
                    list.AddOther(Other[i]);
                }
                else
                {
                    list.AddOther(Other[i].Timestamp, Other[i].Value);
                    list.AddSource(Source[i].Timestamp, Source[i].Value);
                }
            }
        });

    [TestMethod]
    public void PairedAdd_MatchesSeriesExactly()
        => AssertEveryOperation(static list => {
            for (int i = 0; i < Source.Count; i++)
            {
                list.Add(Source[i], Other[i]);
            }
        });

    [TestMethod]
    public void OneSideLags_ResultsArriveWithTheLaggingSide()
    {
        PairMathList list = new(PairOperation.Minus);
        list.AddSource(Source.Take(100).ToList());

        for (int i = 0; i < 100; i++)
        {
            list.AddOther(Other[i]);
            list.Should().HaveCount(i + 1);
        }

        list.IsExactly(Source.Take(100).ToList().Minus(Other.Take(100).ToList()));
    }

    [TestMethod]
    public void UnmatchedTimestamp_ProducesNoResult()
    {
        const int skip = 50;
        List<IReusable> withGap = [.. Other];
        withGap.RemoveAt(skip);

        PairMathList list = new(PairOperation.Plus, Source, withGap);

        list.Should().HaveCount(Source.Count - 1);
        list.Should().NotContain(x => x.Timestamp == Source[skip].Timestamp);

        List<IReusable> sourceWithGap = [.. Source];
        sourceWithGap.RemoveAt(skip);
        list.IsExactly(sourceWithGap.Plus(withGap));
    }

    [TestMethod]
    public void OutOfOrderOnOneSide_Throws()
    {
        PairMathList list = new(PairOperation.Plus, Source.Take(10).ToList(), Other.Take(10).ToList());

        FluentActions.Invoking(() => list.AddSource(Source[5]))
            .Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*source timestamp*not after the last source timestamp*");

        FluentActions.Invoking(() => list.AddOther(Other[9]))
            .Should().ThrowExactly<InvalidOperationException>()
            .WithMessage("*other timestamp*not after the last other timestamp*");

        // a value one side skipped cannot be added late; that needs a StreamHub
        PairMathList gapped = new(PairOperation.Plus);
        gapped.AddSource(Source[0]);
        gapped.AddOther(Other[1]);

        FluentActions.Invoking(() => gapped.AddOther(Other[0]))
            .Should().ThrowExactly<InvalidOperationException>();
    }

    [TestMethod]
    public void Clear_ResetsListAndStagedValues()
    {
        PairMathList list = new(PairOperation.Times);
        list.AddSource(Source);
        list.AddOther(Other.Take(100).ToList());
        list.Should().HaveCount(100);

        list.Clear();
        list.Should().BeEmpty();

        // the staged source values are gone too, and ordering starts over
        list.AddOther(Other);
        list.Should().BeEmpty();
        list.AddSource(Source);
        list.IsExactly(Source.Times(Other));
    }

    [TestMethod]
    public void MaxListSize_PrunesToNewestResults()
    {
        const int maxListSize = 120;

        PairMathList list = new(PairOperation.MaxWith) { MaxListSize = maxListSize };
        list.AddSource(Source);
        list.AddOther(Other);

        list.Should().HaveCount(maxListSize);
        list.IsExactly(Source.MaxWith(Other).TakeLast(maxListSize));
    }

    [TestMethod]
    public void MaxListSize_BoundsTheLeadingSide()
    {
        PairMathList list = new(PairOperation.MinWith) { MaxListSize = 50 };

        // the source runs 100 values ahead; only its newest 50 stay staged
        list.AddSource(Source.Take(100).ToList());
        list.AddOther(Other.Take(100).ToList());

        list.Should().HaveCount(50);
        list.IsExactly(Source.Take(100).ToList().MinWith(Other.Take(100).ToList()).TakeLast(50));
    }

    [TestMethod]
    public void Name_MatchesCatalogId()
    {
        new PairMathList(PairOperation.Plus).Name.Should().Be("PLUS-SERIES");
        new PairMathList(PairOperation.Minus).Name.Should().Be("MINUS-SERIES");
        new PairMathList(PairOperation.Times).Name.Should().Be("TIMES-SERIES");
        new PairMathList(PairOperation.DividedBy).Name.Should().Be("DIVIDED-BY-SERIES");
        new PairMathList(PairOperation.MaxWith).Name.Should().Be("MAX-WITH");
        new PairMathList(PairOperation.MinWith).ToString().Should().Be("MIN-WITH");
    }

    [TestMethod]
    public void InvalidArguments_Throw()
    {
        PairMathList list = new(PairOperation.Plus);

        FluentActions.Invoking(static () => new PairMathList((PairOperation)99))
            .Should().ThrowExactly<ArgumentOutOfRangeException>().WithParameterName("operation");
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
        FluentActions.Invoking(static () => Source.ToPlusList((IReadOnlyList<IReusable>)null!))
            .Should().ThrowExactly<ArgumentNullException>();
    }

    private static void AssertEveryOperation(Action<PairMathList> feed)
    {
        foreach ((PairOperation operation, Func<IReadOnlyList<IReusable>, IReadOnlyList<IReusable>, IReadOnlyList<MathResult>> series, _) in Operations)
        {
            PairMathList list = new(operation);
            feed(list);

            list.IsExactly(series(Source, Other), $"{operation} must match Series");
        }
    }

    private static List<IReusable> WithZero(IReadOnlyList<IReusable> values, int index)
    {
        List<IReusable> list = [.. values];
        list[index] = new TimeValue(values[index].Timestamp, 0);
        return list;
    }
}
