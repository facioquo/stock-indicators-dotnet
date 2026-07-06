namespace Regression;

[TestClass, TestCategory("Regression")]
public class StdDevChannelsRegressionTests : RegressionTestBase<StdDevChannelsResult>
{
    public StdDevChannelsRegressionTests() : base("stdev-channels.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToStdDevChannels(20).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
    // TODO: BufferList implementation not available for StdDevChannels

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => Assert.Inconclusive("Test not yet implemented");
    // TODO: StreamHub implementation not available for StdDevChannels
}
