namespace Utilities;

[TestClass]
public class BinarySettingsTests : TestBase
{
    // see Renko Hub tests for inheritance

    [TestMethod]
    public void Ctor_Default_SetsZeroSettingsAndFullMask()
    {
        BinarySettings sut = new();
        sut.Settings.Should().Be(0);
        sut.Mask.Should().Be(0b11111111);
    }

    [TestMethod]
    public void Ctor_WithSettingsOnly_UsesDefaultMask()
    {
        BinarySettings sut = new(0);
        sut.Settings.Should().Be(0);
        sut.Mask.Should().Be(0b11111111);
    }

    [TestMethod]
    public void Ctor_WithSettingsAndMask_SetsBoth()
    {
        BinarySettings sut = new(0b10101010, 0b11001100);
        sut.Settings.Should().Be(0b10101010);
        sut.Mask.Should().Be(0b11001100);
    }

    [TestMethod]
    public void Indexer_WithBitPosition_ReturnsExpectedBit()
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
    public void Indexer_OutOfRangeIndex_Throws()
    {
        BinarySettings sut = new(0b00000001);

        // without range validation, shift wraparound
        // would silently read bit 0 for index 32
        Action negative = () => _ = sut[-1];
        Action tooHigh = () => _ = sut[8];
        Action wrapped = () => _ = sut[32];

        negative.Should().Throw<ArgumentOutOfRangeException>();
        tooHigh.Should().Throw<ArgumentOutOfRangeException>();
        wrapped.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void Combine_WithDefaultMask_MergesSettings()
    {
        BinarySettings srcSettings = new(0b01101001);
        BinarySettings defSettings = new(0b00000010);
        BinarySettings newSettings = defSettings.Combine(srcSettings);
        newSettings.Settings.Should().Be(0b01101011);
    }

    [TestMethod]
    public void Combine_WithCustomMask_ExcludesMaskedBits()
    {
        BinarySettings srcSettings = new(0b01101001, 0b11111110);
        BinarySettings defSettings = new(0b00000010);
        BinarySettings newSettings = defSettings.Combine(srcSettings);
        newSettings.Settings.Should().Be(0b01101010);
    }

    [TestMethod]
    public void Equality_WithSameAndDifferentValues_ComparesCorrectly()
    {
        BinarySettings sut = new();
        sut.Settings.Should().Be(0b00000000);
        sut.Mask.Should().Be(0b11111111);

        object obj = new BinarySettings(0b01100010);
        BinarySettings sutA = new(0b01100010);
        BinarySettings sutB = new(0b01100010);
        BinarySettings sutC = new(0b01101010); // different

        sutA.Should().Be(sutB);

        // object equality, Equals(object) dispatch
        obj.Equals(sutA).Should().BeTrue();
        sutA.Equals(obj).Should().BeTrue();

        // struct equality, Equals(BinarySettings) overload
        sutA.Equals(sutB).Should().BeTrue();
        sutB.Equals(sutA).Should().BeTrue();
        sutB.Equals(sutC).Should().BeFalse();

        // custom operator equality
        (sutA == sutB).Should().BeTrue();
        (sutA == sutC).Should().BeFalse();
        (sutB != sutC).Should().BeTrue();
    }
}
