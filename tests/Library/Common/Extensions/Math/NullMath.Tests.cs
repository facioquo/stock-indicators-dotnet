namespace Utilities;

[TestClass]
public class NullMaths : TestBase
{
    private readonly double? dblPos = 100.12345;
    private readonly double? dblNeg = -200.98765;
    private readonly decimal? decPos = 10.12345m;
    private readonly decimal? decNeg = -20.98765m;

    [TestMethod]
    public void AbsDouble()
    {
        dblPos.Abs().Should().Be(100.12345d);
        dblNeg.Abs().Should().Be(200.98765d);
        ((double?)null).Abs().Should().BeNull();
    }

    [TestMethod]
    public void Null2NaN()
    {
        // doubles
        dblPos.Null2NaN().Should().Be(100.12345d);
        dblNeg.Null2NaN().Should().Be(-200.98765d);
        ((double?)null).Null2NaN().Should().Be(double.NaN);

        // decimals » doubles
        decPos.Null2NaN().Should().Be(10.12345d);
        decNeg.Null2NaN().Should().Be(-20.98765d);
        ((decimal?)null).Null2NaN().Should().Be(double.NaN);
    }

    [TestMethod]
    public void NaN2Null()
    {
        // double (nullable)
        double? dblNaNul = double.NaN;
        dblNaNul.NaN2Null().Should().BeNull();
        dblPos.NaN2Null().Should().Be(100.12345d);
        dblNeg.NaN2Null().Should().Be(-200.98765d);

        // double (non-nullable)
        const double dblNaN = double.NaN;
        dblNaN.NaN2Null().Should().BeNull();
        100.12345d.NaN2Null().Should().Be(100.12345d);
        (-200.98765d).NaN2Null().Should().Be(-200.98765d);
    }
}
