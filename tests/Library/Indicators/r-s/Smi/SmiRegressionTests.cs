namespace Regression;

[TestClass, TestCategory("Regression")]
public class SmiRegressionTests : RegressionTestBase<SmiResult>
{
    public SmiRegressionTests() : base("smi.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToSmi().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToSmiList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToSmiHub().Results.IsExactly(Expected);
}
