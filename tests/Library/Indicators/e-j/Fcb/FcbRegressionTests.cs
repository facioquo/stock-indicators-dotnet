namespace Regression;

[TestClass, TestCategory("Regression")]
public class FcbRegressionTests : RegressionTestBase<FcbResult>
{
    public FcbRegressionTests() : base("fcb.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToFcb(2).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToFcbList(2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToFcbHub(2).Results.IsExactly(Expected);
}
