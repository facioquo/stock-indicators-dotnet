namespace StreamHubs;

[TestClass]
public class CircularDoubleBufferTests : TestBase
{
    [TestMethod]
    public void CircularDoubleBuffer_GetMax_ReturnsCorrectMax()
    {
        CircularDoubleBuffer buf = new(3);
        buf.Add(10.0);
        buf.Add(30.0);
        buf.Add(20.0);

        buf.GetMax().Should().Be(30.0);
    }

    [TestMethod]
    public void CircularDoubleBuffer_GetMin_ReturnsCorrectMin()
    {
        CircularDoubleBuffer buf = new(3);
        buf.Add(10.0);
        buf.Add(30.0);
        buf.Add(20.0);

        buf.GetMin().Should().Be(10.0);
    }

    [TestMethod]
    public void CircularDoubleBuffer_WrapsAround_MaxMinCorrect()
    {
        CircularDoubleBuffer buf = new(3);
        buf.Add(10.0);
        buf.Add(20.0);
        buf.Add(30.0);
        buf.Add(5.0); // evicts 10, window = [20, 30, 5]

        buf.GetMax().Should().Be(30.0);
        buf.GetMin().Should().Be(5.0);
    }

    [TestMethod]
    public void CircularDoubleBuffer_IsFull_TrueWhenFull()
    {
        CircularDoubleBuffer buf = new(2);
        buf.Add(1.0);
        buf.IsFull.Should().BeFalse();
        buf.Add(2.0);
        buf.IsFull.Should().BeTrue();
    }

    [TestMethod]
    public void CircularDoubleBuffer_IsEmpty_TrueAfterClear()
    {
        CircularDoubleBuffer buf = new(3);
        buf.Add(1.0);
        buf.Clear();
        buf.IsEmpty.Should().BeTrue();
    }

    [TestMethod]
    public void CircularDoubleBuffer_GetMax_ReturnsNaN_WhenEmpty()
    {
        CircularDoubleBuffer buf = new(3);
        buf.GetMax().Should().Be(double.NaN);
    }

    [TestMethod]
    public void CircularDoubleBuffer_GetMin_ReturnsNaN_WhenEmpty()
    {
        CircularDoubleBuffer buf = new(3);
        buf.GetMin().Should().Be(double.NaN);
    }

    [TestMethod]
    public void CircularDoubleBuffer_Clear_ResetsState()
    {
        CircularDoubleBuffer buf = new(3);
        buf.Add(5.0);
        buf.Add(15.0);
        buf.Clear();
        buf.Add(7.0);

        buf.GetMax().Should().Be(7.0);
        buf.GetMin().Should().Be(7.0);
    }
}
