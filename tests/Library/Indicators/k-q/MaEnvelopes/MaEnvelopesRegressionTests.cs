namespace Regression;

[TestClass, TestCategory("Regression")]
public class MaEnvelopesRegressionTests : RegressionTestBase<MaEnvelopeResult>
{
    public MaEnvelopesRegressionTests() : base("ma-env.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToMaEnvelopes(20, 2.5).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToMaEnvelopesList(20, 2.5).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToMaEnvelopesHub(20, 2.5).Results.IsExactly(Expected);
}
