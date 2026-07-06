namespace Regression;

[TestClass, TestCategory("Regression")]
public class AlligatorRegressionTests : RegressionTestBase<AlligatorResult>
{
    public AlligatorRegressionTests() : base("alligator.standard.json") { }

    [TestMethod]
    public override void Series_AgainstBaseline_MatchesExactly() => Bars.ToAlligator().IsExactly(Expected);

    [TestMethod]
    public override void Buffer_AgainstBaseline_MatchesExactly() => Bars.ToAlligatorList().IsExactly(Expected);

    [TestMethod]
    public override void Stream_AgainstBaseline_MatchesExactly() => BarHub.ToAlligatorHub().Results.IsExactly(Expected);
}
