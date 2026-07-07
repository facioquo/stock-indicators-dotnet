namespace Regression;

[TestClass, TestCategory("Regression")]
public class BarPartRegressionTests : RegressionTestBase<TimeValue>
{
    public BarPartRegressionTests() : base("barpart.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToBarPart(CandlePart.Close).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToBarPartList(CandlePart.Close).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToBarPartHub(CandlePart.Close).Results.IsExactly(Expected);
}
