namespace Regression;

[TestClass, TestCategory("Regression")]
public class KvoRegressionTests : RegressionTestBase<KvoResult>
{
    public KvoRegressionTests() : base("kvo.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToKvo(34, 55, 13).IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToKvoList(34, 55, 13).IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly()
    {
        BarHub barHub = new();
        KvoHub hub = barHub.ToKvoHub(34, 55, 13);

        foreach (Bar q in Bars)
        {
            barHub.Add(q);
        }

        hub.Results.IsExactly(Expected);

        hub.Unsubscribe();
        barHub.EndTransmission();
    }
}
