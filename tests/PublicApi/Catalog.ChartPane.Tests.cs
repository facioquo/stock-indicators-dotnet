using System.Text.Json;

namespace Behavioral;

/// <summary>
/// Groups catalog results by chart pane the way a catalog-driven chart does.
/// </summary>
[TestClass]
public class CatalogChartPaneTests
{
    [TestMethod]
    public void MixedScaleListing_GroupsIntoPanes()
    {
        Dictionary<string, string[]> panes = Catalog.Get("BB", Style.Series)!.Results
            .Where(static r => r.ChartPane is not null)
            .GroupBy(static r => r.ChartPane!)
            .ToDictionary(static g => g.Key, static g => g.Select(static r => r.DataName).ToArray());

        panes[IndicatorResult.PricePane].Should().BeEquivalentTo("Sma", "UpperBand", "LowerBand");
        panes.Keys.Should().BeEquivalentTo(IndicatorResult.PricePane, "PercentB", "ZScore", "Width");
    }

    [TestMethod]
    public void JsonExport_NamesPanes()
    {
        using JsonDocument json = JsonDocument.Parse(
            new[] { Catalog.Get("PSAR", Style.Series)! }.ToJson());

        Dictionary<string, JsonElement> panes = json.RootElement[0].GetProperty("results")
            .EnumerateArray()
            .ToDictionary(
                static r => r.GetProperty("dataName").GetString()!,
                static r => r.GetProperty("chartPane"));

        panes["Sar"].GetString().Should().Be(IndicatorResult.PricePane);
        panes["IsReversal"].ValueKind.Should().Be(JsonValueKind.Null);
    }
}
