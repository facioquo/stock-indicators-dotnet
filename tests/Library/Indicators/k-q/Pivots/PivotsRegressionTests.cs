namespace Regression;

[TestClass, TestCategory("Regression")]
public class PivotsRegressionTests : RegressionTestBase<PivotsResult>
{
    public PivotsRegressionTests() : base("pivots.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToPivots().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToPivotsList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub barHub = new();
        barHub.Add(Bars);
        PivotsHub hub = barHub.ToPivotsHub();
        hub.Rebuild(0);  // Calculate trend lines after all pivot points identified
        hub.Results.IsExactly(Expected);
    }
}
