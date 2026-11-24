using System.Reflection;

namespace BufferLists;

/// <summary>
/// A buffer list built on inner buffer lists bounds those inner lists with its own
/// <see cref="BufferList{TResult}.MaxListSize"/> without changing its results, down to
/// a <c>MaxListSize</c> of 1 where it is smaller than an inner list's lookback.
/// <c>StcList</c> and <c>AtrStopList</c> already bounded their inner lists; they are
/// here as regression guards.
/// </summary>
[TestClass]
public class BufferListInnerListsTests : TestBase
{
    private const int roomy = 50;

    // name -> (inner list field -> floor that list is held to, run for one MaxListSize)
    private static readonly Dictionary<string, (Dictionary<string, int> Floors, Action<int> Run)> Cases = new() {
        ["StochRsi"] = Case(new() { ["_rsiList"] = 15, ["_stochList"] = 15 },
            () => new StochRsiList(14, 14, 3, 1), (l, b) => l.Add(b), Bars.ToStochRsi(14, 14, 3, 1)),
        ["StochRsiSignal20"] = Case(new() { ["_rsiList"] = 15, ["_stochList"] = 21 },
            () => new StochRsiList(14, 3, 20, 3), (l, b) => l.Add(b), Bars.ToStochRsi(14, 3, 20, 3)),
        ["StochRsiSmooth3"] = Case(new() { ["_rsiList"] = 15, ["_stochList"] = 15 },
            () => new StochRsiList(14, 14, 3, 3), (l, b) => l.Add(b), Bars.ToStochRsi(14, 14, 3, 3)),
        ["Stc"] = Case(new() { ["_macdList"] = 51 },
            () => new StcList(10, 23, 50), (l, b) => l.Add(b), Bars.ToStc(10, 23, 50)),
        ["Keltner"] = Case(new() { ["_emaList"] = 21, ["_atrList"] = 11 },
            () => new KeltnerList(20, 2, 10), (l, b) => l.Add(b), Bars.ToKeltner(20, 2, 10)),
        ["Chandelier"] = Case(new() { ["_atrList"] = 23 },
            () => new ChandelierList(22, 3), (l, b) => l.Add(b), Bars.ToChandelier(22, 3)),
        ["AtrStop"] = Case(new() { ["_atrList"] = 22 },
            () => new AtrStopList(21, 3), (l, b) => l.Add(b), Bars.ToAtrStop(21, 3)),
        ["ChaikinOsc"] = Case(new() { ["_adlList"] = 2, ["_fastEmaList"] = 4, ["_slowEmaList"] = 11 },
            () => new ChaikinOscList(3, 10), (l, b) => l.Add(b), Bars.ToChaikinOsc(3, 10)),
        ["Cmf"] = Case(new() { ["_adlList"] = 2 },
            () => new CmfList(20), (l, b) => l.Add(b), Bars.ToCmf(20)),
        ["ElderRay"] = Case(new() { ["_emaList"] = 14 },
            () => new ElderRayList(13), (l, b) => l.Add(b), Bars.ToElderRay(13)),
        ["StarcBands"] = Case(new() { ["_smaList"] = 2, ["_atrList"] = 11 },
            () => new StarcBandsList(5, 2, 10), (l, b) => l.Add(b), Bars.ToStarcBands(5, 2, 10)),
        ["SuperTrend"] = Case(new() { ["_atrList"] = 11 },
            () => new SuperTrendList(10, 3), (l, b) => l.Add(b), Bars.ToSuperTrend(10, 3)),
        ["VolatilityStop"] = Case(new() { ["_atrList"] = 8, ["_closePrices"] = 7 },
            () => new VolatilityStopList(7, 3), (l, b) => l.Add(b), Bars.ToVolatilityStop(7, 3)),
        ["Gator"] = Case(new() { ["_alligatorList"] = 2 },
            () => new GatorList(), (l, b) => l.Add(b), Bars.ToGator()),
        ["Pmo"] = Case(new() { ["_rocHistory"] = 35, ["_rocEmaHistory"] = 20, ["_pmoHistory"] = 10 },
            () => new PmoList(35, 20, 10), (l, b) => l.Add(b), Bars.ToPmo(35, 20, 10)),
        ["ConnorsRsi"] = Case(new() { ["_rsiClose"] = 4, ["_rsiStreak"] = 3, ["_gainBuffer"] = 101 },
            () => new ConnorsRsiList(3, 2, 100), (l, b) => l.Add(b), Bars.ToConnorsRsi(3, 2, 100)),
        ["Ichimoku"] = Case(new() { ["_historicalResults"] = 157, ["_historicalHighLow"] = 157 },
            () => new IchimokuList(9, 26, 52), (l, b) => l.Add(b), Bars.ToIchimoku(9, 26, 52)),
        ["VolatilityStopStepwise"] = Case(new() { ["_atrList"] = 8, ["_closePrices"] = 7 },
            () => new VolatilityStopList(7, 3), (l, b) => l.Add(b), Bars.ToVolatilityStop(7, 3),
            stepCheckFrom: Bars.ToVolatilityStop(7, 3).ToList().FindIndex(r => r.Sar is not null) - 1)
    };

    static BufferListInnerListsTests()
    {
        // the inner MA list's floor holds for every moving average type
        foreach (MaType type in new[] {
            MaType.ALMA, MaType.DEMA, MaType.EMA, MaType.EPMA, MaType.HMA,
            MaType.SMA, MaType.SMMA, MaType.TEMA, MaType.WMA })
        {
            MaType t = type;

            Cases[$"MaEnvelopes{t}"] = Case(
                new() { ["_maList"] = 21 },
                () => new MaEnvelopesList(20, 2.5, t),
                (l, b) => l.Add(b),
                Bars.ToMaEnvelopes(20, 2.5, t));
        }
    }

    public static IEnumerable<object[]> Sizes =>
        Cases.SelectMany(c => new[] { 1, 2, Math.Max(3, c.Value.Floors.Values.Max() - 2), roomy }
            .Distinct()
            .Select(m => new object[] { c.Key, m }));

    [TestMethod]
    [DynamicData(nameof(Sizes))]
    public void MaxListSize_AtAnySize_KeepsResultsOfTheSeries(string name, int maxListSize)
        => Cases[name].Run(maxListSize);

    private static (Dictionary<string, int> Floors, Action<int> Run) Case<TList, TResult>(
        Dictionary<string, int> floors,
        Func<TList> make,
        Action<TList, Bar> add,
        IReadOnlyList<TResult> series,
        int stepCheckFrom = int.MaxValue)
        where TList : BufferList<TResult>
        where TResult : ISeries
        => (floors, m => Verify(m, floors, make, add, series, stepCheckFrom));

    private static void Verify<TList, TResult>(
        int maxListSize,
        Dictionary<string, int> floors,
        Func<TList> make,
        Action<TList, Bar> add,
        IReadOnlyList<TResult> series,
        int stepCheckFrom)
        where TList : BufferList<TResult>
        where TResult : ISeries
    {
        if (stepCheckFrom != int.MaxValue)
        {
            stepCheckFrom.Should().BeInRange(0, Bars.Count - 1);
        }

        int expectedCount = Math.Min(maxListSize, Bars.Count);

        // every inner list is listed with its floor, and held to the larger of that and the outer bound
        void AssertInnerBounds(TList sut, string path)
        {
            Dictionary<string, int> counts = typeof(TList)
                .GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(f => (f.Name, Value: f.GetValue(sut)))
                .Where(f => f.Value is List<double> || (f.Value is System.Collections.ICollection && floors.ContainsKey(f.Name)) || (f.Value is not null && IsBufferList(f.Value.GetType())))
                .ToDictionary(
                    f => f.Name,
                    f => f.Value is System.Collections.ICollection c
                        ? c.Count
                        : (int)f.Value.GetType().GetProperty("Count").GetValue(f.Value));

            counts.Keys.Should().BeEquivalentTo(floors.Keys, $"{path} inner lists");

            foreach ((string field, int count) in counts)
            {
                count.Should().BeLessThanOrEqualTo(
                    Math.Max(maxListSize + 1, floors[field]),
                    $"{path} {field}");
            }
        }

        void AssertTail(TList sut, string path)
        {
            sut.Should().HaveCount(expectedCount, path);
            sut.IsExactly(series.Skip(Bars.Count - expectedCount).ToList());
            AssertInnerBounds(sut, path);
        }

        // limit set before any data
        TList sut = make();
        sut.MaxListSize = maxListSize;

        for (int i = 0; i < Bars.Count; i++)
        {
            add(sut, Bars[i]);

            // growth past the bound at any step counts, not only the final state
            AssertInnerBounds(sut, $"incremental step {i}");

            // a list that repaints earlier rows matches the series only once it has stopped repainting;
            // the first non-null row follows the one where the repaint happens
            if (i >= stepCheckFrom)
            {
                sut.IsExactly(series.Skip(i + 1 - sut.Count).Take(sut.Count).ToList());
            }
        }

        AssertTail(sut, "incremental");

        // refill after Clear
        sut.Clear();

        foreach (Bar bar in Bars)
        {
            add(sut, bar);
        }

        AssertTail(sut, "clear and refill");

        // limit set after the data is loaded
        TList late = make();

        foreach (Bar bar in Bars)
        {
            add(late, bar);
        }

        late.MaxListSize = maxListSize;
        AssertTail(late, "limit set after load");
    }

    private static bool IsBufferList(Type type)
    {
        for (Type t = type; t != null; t = t.BaseType)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(BufferList<>))
            {
                return true;
            }
        }

        return false;
    }
}
