namespace Regression;

[TestClass, TestCategory("Regression")]
public class ForceIndexRegressionTests : RegressionTestBase<ForceIndexResult>
{
    public ForceIndexRegressionTests() : base("force.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToForceIndex().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToForceIndexList(2).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToForceIndexHub().Results.IsExactly(Expected);
}
