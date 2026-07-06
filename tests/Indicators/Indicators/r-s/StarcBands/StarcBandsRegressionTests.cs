namespace Regression;

[TestClass, TestCategory("Regression")]
public class StarcBandsRegressionTests : RegressionTestBase<StarcBandsResult>
{
    public StarcBandsRegressionTests() : base("starc.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToStarcBands().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToStarcBandsList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToStarcBandsHub().Results.IsExactly(Expected);
}
