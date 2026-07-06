namespace Observables;

/// <summary>
/// Pins the root-hub mutation contract: only a root hub (a standalone
/// <see cref="BarHub"/> or <see cref="TradeTickHub"/>) may have its input timeline
/// mutated directly. A subscribed (chained) hub is driven by its provider, so
/// the public mutating surface — Add, RemoveAt, RemoveRange, Reinitialize, and
/// the Remove(IBar/ITradeTick) convenience — is rejected on it. Also covers the
/// Remove-by-timestamp convenience on the root hub.
/// </summary>
[TestClass]
public class MutationGuard : TestBase
{
    // ROOT HUB — mutation allowed

    [TestMethod]
    public void RootHub_AllowsMutation()
    {
        BarHub barHub = new();

        // single + batch add
        barHub.Add(Bars[0]);
        barHub.Add(Bars.Skip(1).Take(49));
        barHub.Bars.Should().HaveCount(50);

        // remove by index
        barHub.RemoveAt(49);
        barHub.Bars.Should().HaveCount(49);

        // remove range by timestamp
        barHub.RemoveRange(Bars[40].Timestamp, notify: true);
        barHub.Bars.Should().HaveCount(40);

        // remove range by index
        barHub.RemoveRange(30, notify: true);
        barHub.Bars.Should().HaveCount(30);

        barHub.EndTransmission();
    }

    [TestMethod]
    public void ConstructionTimeReinitialize_IsAllowed()
    {
        // every indicator hub calls Reinitialize() from its constructor; the
        // guard must not break that. Build a chained hub and confirm it warms
        // up correctly (proving the construction-time Reinitialize ran).
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        barHub.Add(Bars.Take(50));

        observer.Results.Should().HaveCount(50);
        observer.Results.IsExactly(Bars.Take(50).ToList().ToSma(20));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void Reinitialize_OnRootHub_RebuildsCleanlyWithoutThrowing()
    {
        // A root hub's provider is inert, so Reinitialize() must be a clean
        // rebuild that re-notifies dependents — not a partial run that throws
        // from the inert provider's Subscribe.
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        barHub.Add(Bars.Take(50));
        observer.Results.Should().HaveCount(50);

        Action act = barHub.Reinitialize;
        act.Should().NotThrow();

        // cache intact and the dependent stayed in sync
        barHub.Bars.Should().HaveCount(50);
        observer.Results.IsExactly(Bars.Take(50).ToList().ToSma(20));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    // SUBSCRIBED HUB — mutation rejected

    [TestMethod]
    public void SubscribedHub_Add_Throws()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);
        barHub.Add(Bars.Take(50));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.Add(Bars[50]));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.Add(Bars.Skip(50).Take(5)));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SubscribedHub_RemoveAt_Throws()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);
        barHub.Add(Bars.Take(50));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveAt(30));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SubscribedHub_RemoveRange_Throws()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);
        barHub.Add(Bars.Take(50));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveRange(Bars[30].Timestamp, notify: true));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.RemoveRange(30, notify: true));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SubscribedHub_Reinitialize_Throws()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);
        barHub.Add(Bars.Take(50));

        Assert.ThrowsExactly<InvalidOperationException>(observer.Reinitialize);

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void SubscribedHub_StaysInSyncDespiteGuard()
    {
        // the guard must not interfere with normal provider-driven operation:
        // late arrivals and root-hub removals still cascade to the observer.
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        List<Bar> bars = Bars.Take(200).ToList();

        // feed all but one, then add the skipped bar late
        for (int i = 0; i < bars.Count; i++)
        {
            if (i == 80) { continue; }

            barHub.Add(bars[i]);
        }

        barHub.Add(bars[80]);     // late arrival → rollback + replay

        const int removeIndex = 120;
        barHub.RemoveAt(removeIndex);  // remove from root → cascade

        List<Bar> expected = [.. bars];
        expected.RemoveAt(removeIndex);
        observer.Results.IsExactly(expected.ToSma(20));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    // Remove(IBar) convenience — root BarHub

    [TestMethod]
    public void RemoveByBar_RemovesByTimestamp_CascadesToObserver()
    {
        BarHub barHub = new();
        SmaHub observer = barHub.ToSmaHub(20);

        List<Bar> bars = Bars.Take(50).ToList();
        barHub.Add(bars);

        // a proxy bar at the same timestamp (different values) must still
        // match — Remove locates the entry by timestamp, not by equality
        Bar proxy = bars[30] with { Close = bars[30].Close + 5m };
        barHub.Remove(proxy);

        barHub.Bars.Should().HaveCount(49);

        List<Bar> expected = [.. bars];
        expected.RemoveAt(30);
        observer.Results.IsExactly(expected.ToSma(20));

        observer.Unsubscribe();
        barHub.EndTransmission();
    }

    [TestMethod]
    public void RemoveByBar_AbsentTimestamp_Throws()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(50));

        // a timestamp not present in the cache
        Bar absent = Bars[0] with { Timestamp = Bars[0].Timestamp.AddTicks(-1) };

        Assert.ThrowsExactly<ArgumentException>(() => barHub.Remove(absent));

        barHub.EndTransmission();
    }

    [TestMethod]
    public void RemoveByBar_Null_Throws()
    {
        BarHub barHub = new();
        barHub.Add(Bars.Take(10));

        Assert.ThrowsExactly<ArgumentNullException>(() => barHub.Remove(null!));

        barHub.EndTransmission();
    }

    [TestMethod]
    public void RemoveByBar_OnSubscribedHub_Throws()
    {
        BarHub provider = new();
        BarHub observer = provider.ToBarHub();
        provider.Add(Bars.Take(50));

        Assert.ThrowsExactly<InvalidOperationException>(
            () => observer.Remove(Bars[30]));

        observer.Unsubscribe();
        provider.EndTransmission();
    }

    // Remove(ITradeTick) convenience — root TradeTickHub

    [TestMethod]
    public void RemoveByTradeTick_RemovesByTimestamp()
    {
        DateTime start = new(2023, 11, 9, 10, 0, 0, DateTimeKind.Utc);

        List<TradeTick> ticks = [];
        for (int i = 0; i < 30; i++)
        {
            ticks.Add(new TradeTick(start.AddMinutes(i), 100m + i, 10m + i));
        }

        TradeTickHub tickHub = new();
        tickHub.Add(ticks);
        tickHub.Cache.Should().HaveCount(30);

        tickHub.Remove(ticks[15]);
        tickHub.Cache.Should().HaveCount(29);
        tickHub.Cache.Any(t => t.Timestamp == ticks[15].Timestamp).Should().BeFalse();

        // absent timestamp throws
        TradeTick absent = new(start.AddMinutes(-1), 1m, 1m);
        Assert.ThrowsExactly<ArgumentException>(() => tickHub.Remove(absent));

        tickHub.EndTransmission();
    }
}
