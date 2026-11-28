namespace Tests.Indicators;

[TestClass]
public class AlligatorTests : TestBase
{
    [TestMethod]
    public void Standard()
    {
        List<AlligatorResult> results = quotes
            .GetAlligator()
            .ToList();

        // proper quantities
        Assert.HasCount(502, results);
        Assert.AreEqual(482, results.Count(static x => x.Jaw != null));
        Assert.AreEqual(490, results.Count(static x => x.Teeth != null));
        Assert.AreEqual(495, results.Count(static x => x.Lips != null));

        // starting calculations at proper index
        Assert.IsNull(results[19].Jaw);
        Assert.IsNotNull(results[20].Jaw);

        Assert.IsNull(results[11].Teeth);
        Assert.IsNotNull(results[12].Teeth);

        Assert.IsNull(results[6].Lips);
        Assert.IsNotNull(results[7].Lips);

        // sample values
        Assert.AreEqual(213.81269, results[20].Jaw.Round(5));
        Assert.AreEqual(213.79287, results[21].Jaw.Round(5));
        Assert.AreEqual(225.60571, results[99].Jaw.Round(5));
        Assert.AreEqual(260.98953, results[501].Jaw.Round(5));

        Assert.AreEqual(213.699375, results[12].Teeth.Round(6));
        Assert.AreEqual(213.80008, results[13].Teeth.Round(5));
        Assert.AreEqual(226.12157, results[99].Teeth.Round(5));
        Assert.AreEqual(253.53576, results[501].Teeth.Round(5));

        Assert.AreEqual(213.63500, results[7].Lips.Round(5));
        Assert.AreEqual(213.74900, results[8].Lips.Round(5));
        Assert.AreEqual(226.35353, results[99].Lips.Round(5));
        Assert.AreEqual(244.29591, results[501].Lips.Round(5));
    }

    [TestMethod]
    public void UseTuple()
    {
        List<AlligatorResult> results = quotes
            .Use(CandlePart.HL2)
            .GetAlligator()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(482, results.Count(static x => x.Jaw != null));

        AlligatorResult last = results.LastOrDefault();
        Assert.AreEqual(244.29591, last.Lips.Round(5));
    }

    [TestMethod]
    public void TupleNaN()
    {
        List<AlligatorResult> r = tupleNanny
            .GetAlligator()
            .ToList();

        Assert.HasCount(200, r);
        Assert.IsEmpty(r.Where(static x => x.Lips is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void Chainee()
    {
        List<AlligatorResult> results = quotes
            .GetSma(2)
            .GetAlligator()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(481, results.Count(static x => x.Jaw != null));
    }

    [TestMethod]
    public void Sync()
    {
        List<AlligatorResult> results = quotes
            .GetSma(3)
            .GetAlligator()
            .ToList();

        Assert.HasCount(502, results);
        Assert.AreEqual(480, results.Count(static x => x.Jaw != null));
    }

    [TestMethod]
    public void BadData()
    {
        List<AlligatorResult> r = badQuotes
            .GetAlligator(3, 3, 2, 1, 1, 1)
            .ToList();

        Assert.HasCount(502, r);
        Assert.IsEmpty(r.Where(static x => x.Jaw is double v && double.IsNaN(v)));
    }

    [TestMethod]
    public void NoQuotes()
    {
        List<AlligatorResult> r0 = noquotes
            .GetAlligator()
            .ToList();

        Assert.IsEmpty(r0);

        List<AlligatorResult> r1 = onequote
            .GetAlligator()
            .ToList();

        Assert.HasCount(1, r1);
    }

    [TestMethod]
    public void Condense()
    {
        List<AlligatorResult> r = quotes
            .GetAlligator()
            .Condense()
            .ToList();

        Assert.HasCount(495, r);

        AlligatorResult last = r.LastOrDefault();
        Assert.AreEqual(260.98953, last.Jaw.Round(5));
        Assert.AreEqual(253.53576, last.Teeth.Round(5));
        Assert.AreEqual(244.29591, last.Lips.Round(5));
    }

    [TestMethod]
    public void Removed()
    {
        List<AlligatorResult> r = quotes
            .GetAlligator(13, 8)
            .RemoveWarmupPeriods()
            .ToList();

        Assert.HasCount(502 - 21 - 250, r);

        AlligatorResult last = r.LastOrDefault();
        Assert.AreEqual(260.98953, last.Jaw.Round(5));
        Assert.AreEqual(253.53576, last.Teeth.Round(5));
        Assert.AreEqual(244.29591, last.Lips.Round(5));
    }

    [TestMethod]
    public void Exceptions()
    {
        // bad jaw lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 13, 5, 5, 3));

        // bad teeth lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 8, 5, 8, 3));

        // bad lips lookback periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 8, 5, 0, 3));

        // bad jaw offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 0, 8, 5, 5, 3));

        // bad teeth offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 8, 0, 5, 3))
            .Message.Should().StartWith("Teeth offset periods");

        // bad lips offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 8, 5, 5, 0))
            .Message.Should().StartWith("Lips offset periods");

        // bad jaw + offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 12, 11, 5, 3));

        // bad teeth + offset periods
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(
            static () => quotes.GetAlligator(13, 8, 8, 5, 7, 7));
    }
}
