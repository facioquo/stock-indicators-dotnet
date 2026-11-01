namespace Utilities;

[TestClass]
public class PairStagerTests : TestBase
{
    private static readonly DateTime T0 = new(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

    private static DateTime At(int day) => T0.AddDays(day);

    [TestMethod]
    public void TryGetPair_BothSidesHoldTimestamp_ReturnsMatchedValues()
    {
        PairStager stager = new(10);
        stager.Set(PairSide.Source, At(0), 1);
        stager.Set(PairSide.Other, At(0), 10);

        stager.TryGetPair(At(0), out PairedValue pair).Should().BeTrue();
        pair.Should().Be(new PairedValue(At(0), 1, 10));
    }

    [TestMethod]
    public void TryGetPair_OneSideOnly_ReturnsFalse()
    {
        PairStager stager = new(10);
        stager.Set(PairSide.Source, At(0), 1);
        stager.Set(PairSide.Other, At(1), 10);

        stager.TryGetPair(At(0), out PairedValue sourceOnly).Should().BeFalse();
        sourceOnly.Should().BeNull();
        stager.TryGetPair(At(1), out PairedValue otherOnly).Should().BeFalse();
        otherOnly.Should().BeNull();
    }

    [TestMethod]
    public void Set_NaN_IsKeptAsIs()
    {
        PairStager stager = new(10);
        stager.Set(PairSide.Source, At(0), double.NaN);
        stager.Set(PairSide.Other, At(0), 2);

        stager.TryGetPair(At(0), out PairedValue pair).Should().BeTrue();
        pair.Value.Should().Be(double.NaN);
        pair.Other.Should().Be(2);
    }

    [TestMethod]
    public void Set_SameTimestamp_ReplacesValue()
    {
        PairStager stager = new(10);
        stager.Set(PairSide.Source, At(0), 1);
        stager.Set(PairSide.Source, At(1), 2);
        stager.Set(PairSide.Source, At(0), 5);
        stager.Set(PairSide.Source, At(1), 6);
        stager.Set(PairSide.Other, At(0), 0);
        stager.Set(PairSide.Other, At(1), 0);

        stager.Count(PairSide.Source).Should().Be(2);
        stager.TryGetPair(At(0), out PairedValue first).Should().BeTrue();
        first.Value.Should().Be(5);
        stager.TryGetPair(At(1), out PairedValue last).Should().BeTrue();
        last.Value.Should().Be(6);
    }

    [TestMethod]
    public void Set_OlderTimestamp_InsertsInOrder()
    {
        PairStager stager = new(10);
        stager.Set(PairSide.Other, At(0), 0);
        stager.Set(PairSide.Other, At(2), 2);
        stager.Set(PairSide.Other, At(1), 1);
        stager.Set(PairSide.Source, At(1), 9);

        stager.Count(PairSide.Other).Should().Be(3);
        stager.Last(PairSide.Other).Should().Be(At(2));
        stager.TryGetPair(At(1), out PairedValue pair).Should().BeTrue();
        pair.Should().Be(new PairedValue(At(1), 9, 1));
    }

    [TestMethod]
    public void Set_BeyondMaxCount_DropsOldest()
    {
        PairStager stager = new(3);

        for (int i = 0; i < 5; i++)
        {
            stager.Set(PairSide.Source, At(i), i);
            stager.Set(PairSide.Other, At(i), i);
        }

        stager.Count(PairSide.Source).Should().Be(3);
        stager.Count(PairSide.Other).Should().Be(3);
        stager.TryGetPair(At(1), out _).Should().BeFalse();
        stager.TryGetPair(At(2), out _).Should().BeTrue();

        // an insert that overflows also drops the oldest
        stager.Set(PairSide.Other, At(3).AddHours(12), 0);
        stager.Count(PairSide.Other).Should().Be(3);
        stager.TryGetPair(At(2), out _).Should().BeFalse();
    }

    [TestMethod]
    public void Last_EmptySide_ReturnsNull()
    {
        PairStager stager = new(10);
        stager.Last(PairSide.Source).Should().BeNull();
        stager.Count(PairSide.Source).Should().Be(0);
    }

    [TestMethod]
    public void RemoveFrom_RemovesTimestampAndLater()
    {
        PairStager stager = Filled(5);

        stager.RemoveFrom(PairSide.Source, At(2));
        stager.Last(PairSide.Source).Should().Be(At(1));
        stager.Count(PairSide.Other).Should().Be(5);

        // a timestamp past the newest removes nothing
        stager.RemoveFrom(PairSide.Source, At(9));
        stager.Count(PairSide.Source).Should().Be(2);
    }

    [TestMethod]
    public void RemoveAfter_KeepsTimestampAndEarlier()
    {
        PairStager stager = Filled(5);

        stager.RemoveAfter(PairSide.Other, At(2));
        stager.Last(PairSide.Other).Should().Be(At(2));

        // a timestamp between values keeps the earlier ones
        stager.RemoveAfter(PairSide.Other, At(1).AddHours(12));
        stager.Last(PairSide.Other).Should().Be(At(1));

        // a timestamp past the newest removes nothing
        stager.RemoveAfter(PairSide.Other, At(9));
        stager.Count(PairSide.Other).Should().Be(2);

        // a timestamp before the oldest removes everything
        stager.RemoveAfter(PairSide.Other, At(-1));
        stager.Count(PairSide.Other).Should().Be(0);
    }

    [TestMethod]
    public void RemoveBefore_KeepsTimestampAndLater()
    {
        PairStager stager = Filled(5);

        stager.RemoveBefore(PairSide.Source, At(2));
        stager.Count(PairSide.Source).Should().Be(3);
        stager.TryGetPair(At(1), out _).Should().BeFalse();
        stager.TryGetPair(At(2), out _).Should().BeTrue();

        // a timestamp past the newest removes everything
        stager.RemoveBefore(PairSide.Source, At(9));
        stager.Count(PairSide.Source).Should().Be(0);
    }

    [TestMethod]
    public void Clear_EmptiesBothSides()
    {
        PairStager stager = Filled(3);
        stager.Clear();

        stager.Count(PairSide.Source).Should().Be(0);
        stager.Count(PairSide.Other).Should().Be(0);
    }

    [TestMethod]
    public void Constructor_MaxCountBelowOne_Throws()
        => FluentActions.Invoking(static () => new PairStager(0))
            .Should().ThrowExactly<ArgumentOutOfRangeException>()
            .WithParameterName("maxCount");

    private static PairStager Filled(int count)
    {
        PairStager stager = new(10);

        for (int i = 0; i < count; i++)
        {
            stager.Set(PairSide.Source, At(i), i);
            stager.Set(PairSide.Other, At(i), -i);
        }

        return stager;
    }
}
