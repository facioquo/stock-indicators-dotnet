namespace Regression;

[TestClass, TestCategory("Regression")]
public class GatorRegressionTests : RegressionTestBase<GatorResult>
{
    public GatorRegressionTests() : base("gator.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToGator().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToGatorList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Bars.ToGatorHub().Results.IsExactly(Expected);
}
