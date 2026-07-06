namespace Regression;

[TestClass, TestCategory("Regression")]
public class StcRegressionTests : RegressionTestBase<StcResult>
{
    public StcRegressionTests() : base("stc.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToStc(10, 23, 50).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToStcList(10, 23, 50).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub hub = new();
        hub.Add(Bars);
        hub.ToStcHub(10, 23, 50).Results.IsExactly(Expected);
    }
}
