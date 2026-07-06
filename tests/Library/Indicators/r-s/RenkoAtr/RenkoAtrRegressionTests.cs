namespace Regression;

[TestClass, TestCategory("Regression")]
public class RenkoAtrRegressionTests : RegressionTestBase<RenkoResult>
{
    public RenkoAtrRegressionTests() : base("renko-atr.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToRenkoAtr().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
    // TODO: BufferList implementation not available for RenkoAtr

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
    // TODO: StreamHub implementation not available for RenkoAtr
}
