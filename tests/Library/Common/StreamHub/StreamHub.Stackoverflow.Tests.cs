namespace Observables;

[TestClass]
public class Stackoverflow : TestBase
{
    [TestMethod]
    public void FatLongStack()
    {
        // goal: about ~10 subscribers, with really long
        // bar history, checking for stack overflow

        const int qtyBars = 20000;

        // setup: many random bars (massive)
        IReadOnlyList<Bar> barsList = Data.GetRandom(qtyBars);

        BarHub barHub = new();

        // setup: define ~10 subscribers (flat)
        List<(string label, IReadOnlyList<ISeries> sut, bool irregular)> subscribers =
        [
            HubRef(barHub.ToAdlHub()),
            HubRef(barHub.ToEmaHub(14))
        ];

        // all USEs
        foreach (CandlePart candlePart in Enum.GetValues<CandlePart>())
        {
            subscribers.Add(HubRef(barHub.ToBarPartHub(candlePart)));
        }

        // act: add bars
        for (int i = 0; i < qtyBars; i++)
        {
            barHub.Add(barsList[i]);
        }

        subscribers.Insert(0, new(barHub.ToString(), barHub.Bars, false));

        // assert: this just has to not fail, really

        Console.WriteLine($"Subscribers: {subscribers.Count}");
        Console.WriteLine("--------------------");

        // assert: all non-irregular subscribers have the same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Hub: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(qtyBars);
        }

        // assert: [last subscriber] has the same dates
        IReadOnlyList<ISeries> lastSubscriber = subscribers[^1].sut.ToList();
        for (int i = 0; i < qtyBars; i++)
        {
            Bar q = barsList[i];
            ISeries r = lastSubscriber[i];
            r.Timestamp.Should().Be(q.Timestamp);
        }

        // act: clear barHub cache (cascades to subscribers)
        const int cutoff = qtyBars / 2;
        barHub.RemoveRange(cutoff, notify: true);

        barHub.Bars.Count.Should().Be(cutoff);

        Console.WriteLine("--------------------");

        // assert: all have same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Cut: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(cutoff);
        }
    }

    [TestMethod]
    public void ManyChainDepths()
    {
        // goal: test that a massive chain where each new subscriber
        // subscribes to the next creating a really long chain
        // of observers, without stack overflow

        const int qtyBars = 10000;
        const int chainDepth = 500;

        // setup: many random bars (massive)
        IReadOnlyList<Bar> barsList = Data.GetRandom(qtyBars);

        BarHub barHub = new();

        // setup: subscribe a large chain depth
        List<(string label, IReadOnlyList<ISeries> sut, bool irregular)> subscribers = new(chainDepth + 2);

        SmaHub init = barHub.ToSmaHub(1);
        SmaHub sma = init.ToSmaHub(2);

        subscribers.Add(HubRef(init));
        subscribers.Add(HubRef(sma));

        int lookbackPeriods = 1;

        // recursive providers
        for (int i = 1; i <= chainDepth; i++)
        {
            sma = sma.ToSmaHub(lookbackPeriods);
            subscribers.Add(HubRef(sma));

            lookbackPeriods = lookbackPeriods is 2 ? 1 : 2;
        }

        // act: add bars
        for (int i = 0; i < qtyBars; i++)
        {
            barHub.Add(barsList[i]);
        }

        subscribers.Insert(0, new(barHub.ToString(), barHub.Bars, false));

        Console.WriteLine($"Subscribers: {subscribers.Count}");
        Console.WriteLine("--------------------");

        // assert: this just has to not fail, really

        // assert: all non-irregular subscribers have the same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Hub: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(qtyBars);
        }

        // assert: [last subscriber] has the same dates
        IReadOnlyList<ISeries> lastSubscriber = subscribers[^1].sut.ToList();
        for (int i = 0; i < qtyBars; i++)
        {
            Bar q = barsList[i];
            ISeries r = lastSubscriber[i];
            r.Timestamp.Should().Be(q.Timestamp);
        }

        // act: clear barHub cache (cascades to subscribers)
        const int cutoff = qtyBars / 2;
        barHub.RemoveRange(cutoff, notify: true);

        barHub.Bars.Count.Should().Be(cutoff);

        // assert: all have same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Cut: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(cutoff);
        }
    }

    [TestMethod]
    public void ManySubscribers()
    {
        // goal: test that many indictors (all at once)
        // can subscribe to the same bar barHub
        // without stack overflow; ~350 subscribers

        const int qtyBars = 5000;

        // setup: many random bars
        IReadOnlyList<Bar> barsList = Data.GetRandom(qtyBars);

        BarHub barHub = new();

        // setup: define all possible subscribers
        // TODO: add to this as more Hubs come online
        List<(string label, IReadOnlyList<ISeries> sut, bool irregular)> subscribers =
        [
            HubRef(barHub.ToAdlHub()),
            HubRef(barHub.ToAlligatorHub()),
            HubRef(barHub.ToEmaHub(14)),
            //HubRef(barHub.ToRenko(2.1m), irregular: true),
            HubRef(barHub.ToBarHub())
        ];

        // all BarParts
        foreach (CandlePart candlePart in Enum.GetValues<CandlePart>())
        {
            subscribers.Add(HubRef(barHub.ToBarPartHub(candlePart)));
        }

        // many SMAs
        for (int i = 1; i <= 300; i++)
        {
            subscribers.Add(HubRef(barHub.ToSmaHub(i)));
        }

        // act: add bars
        for (int i = 0; i < qtyBars; i++)
        {
            barHub.Add(barsList[i]);
        }

        subscribers.Insert(0, new(barHub.ToString(), barHub.Bars, false));

        // assert: this just has to not fail, really

        Console.WriteLine($"Subscribers: {subscribers.Count}");
        Console.WriteLine("--------------------");

        // assert: all non-irregular subscribers have the same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Hub: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(qtyBars);
        }

        // assert: [last subscriber] has the same dates
        IReadOnlyList<ISeries> lastSubscriber = subscribers[^1].sut.ToList();
        for (int i = 0; i < qtyBars; i++)
        {
            Bar q = barsList[i];
            ISeries r = lastSubscriber[i];
            r.Timestamp.Should().Be(q.Timestamp);
        }

        // act: clear barHub cache (cascades to subscribers)
        const int cutoff = qtyBars / 2;
        barHub.RemoveRange(cutoff, notify: true);

        barHub.Bars.Count.Should().Be(cutoff);

        Console.WriteLine("--------------------");

        // assert: all have same count
        foreach ((string label, IReadOnlyList<ISeries> sut, bool irregular) in subscribers)
        {
            int resultQty = sut.Count;
            Console.WriteLine($"Cut: {resultQty} - {label}");
            if (irregular) { continue; }

            resultQty.Should().Be(cutoff);
        }
    }

    /// <summary>
    /// Utility to get references to a hub's sut.
    /// </summary>
    /// <param name="hub">Stream hub</param>
    /// <param name="irregular">Is not normal</param>
    private static (string, IReadOnlyList<TOut>, bool) HubRef<TIn, TOut>(
        StreamHub<TIn, TOut> hub, bool irregular = false)
        where TIn : ISeries
        where TOut : ISeries
    {
        IReadOnlyList<TOut> sut = hub.Cache;
        return (hub.ToString(), sut, irregular);
    }
}
