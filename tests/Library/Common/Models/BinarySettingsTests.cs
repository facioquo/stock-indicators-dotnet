namespace Utilities;

[TestClass]
public class BinarySettingsTests : TestBase
{
    // see Renko Hub tests for inheritance

    [TestMethod]
    public void InitializationDefault()
    {
        BinarySettings sut = new();
        sut.Settings.Should().Be(0);
        sut.Mask.Should().Be(0b11111111);
    }

    [TestMethod]
    public void InitializationPartial()
    {
        BinarySettings sut = new(0);
        sut.Settings.Should().Be(0);
        sut.Mask.Should().Be(0b11111111);
    }

    [TestMethod]
    public void InitializationCustom()
    {
        BinarySettings sut = new(0b10101010, 0b11001100);
        sut.Settings.Should().Be(0b10101010);
        sut.Mask.Should().Be(0b11001100);
    }

    [TestMethod]
    public void AccessBit()
    {
        BinarySettings sut = new(0b00010001);

        // positions: 76543210
        sut[0].Should().BeTrue();
        sut[1].Should().BeFalse();
        sut[2].Should().BeFalse();
        sut[3].Should().BeFalse();
        sut[4].Should().BeTrue();
        sut[5].Should().BeFalse();
        sut[6].Should().BeFalse();
        sut[7].Should().BeFalse();
    }

    [TestMethod]
    public void CombineDefaultMask()
    {
        BinarySettings srcSettings = new(0b01101001);
        BinarySettings defSettings = new(0b00000010);
        BinarySettings newSettings = defSettings.Combine(srcSettings);
        newSettings.Settings.Should().Be(0b01101011);
    }

    [TestMethod]
    public void CombineCustomMask()
    {
        BinarySettings srcSettings = new(0b01101001, 0b11111110);
        BinarySettings defSettings = new(0b00000010);
        BinarySettings newSettings = defSettings.Combine(srcSettings);
        newSettings.Settings.Should().Be(0b01101010);
    }

    [TestMethod]
    public void Equality()
    {
        BinarySettings sut = new();
        Assert.AreEqual(0b00000000, sut.Settings);
        Assert.AreEqual(0b11111111, sut.Mask);

        object obj = new BinarySettings(0b01100010);
        BinarySettings sutA = new(0b01100010);
        BinarySettings sutB = new(0b01100010);
        BinarySettings sutC = new(0b01101010); // different

        Assert.AreEqual(sutA, sutB);

        // object equality
        Assert.IsTrue(obj.Equals(sutA));
        Assert.IsTrue(sutA.Equals(obj));

        // struct equality
        Assert.IsTrue(sutA.Equals(sutB));
        Assert.IsTrue(sutB.Equals(sutA));
        Assert.IsFalse(sutB.Equals(sutC));

        // operator equality
        Assert.IsTrue(sutA == sutB);
        Assert.IsFalse(sutA == sutC);
        Assert.IsTrue(sutB != sutC);
    }
}
