namespace Utilities;

// bars equality

public partial class Bars : TestBase
{
    [TestMethod]
    public void Equality()
    {
        Bar q1 = new(EvalDate, 1m, 1m, 1m, 1m, 100);
        Bar q2 = new(EvalDate, 1m, 1m, 1m, 1m, 100);
        Bar q3 = new(EvalDate, 1m, 1m, 1m, 2m, 99);

        Assert.IsTrue(Equals(q1, q2));
        Assert.IsFalse(Equals(q1, q3));

        Assert.IsTrue(q1.Equals(q2));
        Assert.IsFalse(q1.Equals(q3));

        (q1 == q2).Should().BeTrue();
        (q1 == q3).Should().BeFalse();

        (q1 != q2).Should().BeFalse();
        (q1 != q3).Should().BeTrue();
    }
}
