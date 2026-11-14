using System.Text.Json;

namespace Utilities;

[TestClass]
public class BarJsonTests : TestBase
{
    private static readonly Bar Sample = new(
        new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc), 100m, 102m, 99m, 101m, 1500m);

    [TestMethod]
    public void Serialize_ObsoleteDateAlias_IsOmitted()
    {
        string json = JsonSerializer.Serialize(Sample);

        using JsonDocument doc = JsonDocument.Parse(json);

        doc.RootElement.TryGetProperty(nameof(Bar.Timestamp), out _).Should().BeTrue();
        doc.RootElement.TryGetProperty("Date", out _).Should().BeFalse();
        doc.RootElement.TryGetProperty(nameof(Bar.Value), out _).Should().BeFalse();
    }

    [TestMethod]
    public void Deserialize_GeneratedContextWithoutDate_ReadsTimestamp()
    {
        const string json = """
            [{"Timestamp":"2026-01-02T00:00:00Z","Open":100,"High":102,"Low":99,"Close":101,"Volume":1500}]
            """;

        IReadOnlyList<Bar> actual = JsonSerializer.Deserialize(json, IndicatorJsonContext.Default.IReadOnlyListBar)!;

        actual.Should().Equal(Sample);
    }

    [TestMethod]
    public void Deserialize_OnlyObsoleteDate_LeavesTimestampDefault()
    {
        const string json = """
            {"Date":"2026-01-02T00:00:00Z","Open":100,"High":102,"Low":99,"Close":101,"Volume":1500}
            """;

        Bar actual = JsonSerializer.Deserialize<Bar>(json)!;

        actual.Timestamp.Should().Be(default);
    }

    [TestMethod]
    public void Deserialize_GeneratedContext_RoundTrips()
    {
        string json = JsonSerializer.Serialize(Bars, IndicatorJsonContext.Default.IReadOnlyListBar);

        IReadOnlyList<Bar> actual = JsonSerializer.Deserialize(json, IndicatorJsonContext.Default.IReadOnlyListBar)!;

        actual.Should().Equal(Bars);
    }
}
