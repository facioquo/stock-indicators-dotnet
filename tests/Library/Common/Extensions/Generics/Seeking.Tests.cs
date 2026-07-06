namespace Utilities;

[TestClass]
public class Seeking : TestBaseWithPrecision
{
    [TestMethod]
    public void Find()
    {
        IReadOnlyList<EmaResult> emaResults = Bars.ToEma(20);

        // find specific date
        DateTime findDate = DateTime.ParseExact("2018-12-31", "yyyy-MM-dd", invariantCulture);

        EmaResult r = emaResults.Find(findDate);
        r.Ema.Should().BeApproximately(249.351896680, Money8);
    }
}
