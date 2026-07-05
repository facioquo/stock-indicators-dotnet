namespace Regression;

[TestClass, TestCategory("Regression")]
public class ChandelierRegressionTests : RegressionTestBase<ChandelierResult>
{
    public ChandelierRegressionTests() : base("chexit.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToChandelier(22, 3).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToChandelierList(22, 3).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub hub = new();
        hub.Add(Bars);
        hub.ToChandelierHub(22, 3).Results.IsExactly(Expected);
        hub.EndTransmission();
    }
}
