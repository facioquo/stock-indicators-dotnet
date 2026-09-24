using System.Globalization;
using System.Text.Json;

namespace Behavioral;

/// <summary>
/// Reads enum parameter options the way a catalog-driven consumer does: from the style
/// listings <c>Catalog.Get()</c> returns, and from their JSON export.
/// </summary>
[TestClass]
public class CatalogEnumOptionsTests
{
    [TestMethod]
    public void StyleListings_ListEnumValues()
    {
        foreach (Style style in Enum.GetValues<Style>())
        {
            IndicatorParam endType = Catalog.Get("RENKO", style)!.Parameters!
                .Single(static p => p.ParameterName == "endType");

            endType.DataType.Should().Be("enum");
            endType.EnumOptions.Should().Equal(new Dictionary<int, string> {
                [(int)EndType.Close] = nameof(EndType.Close),
                [(int)EndType.HighLow] = nameof(EndType.HighLow)
            });
        }
    }

    [TestMethod]
    public void JsonExport_ListsEnumValues()
    {
        using JsonDocument json = JsonDocument.Parse(
            new[] { Catalog.Get("RENKO", Style.Series)! }.ToJson());

        JsonElement endType = json.RootElement[0].GetProperty("parameters")
            .EnumerateArray()
            .Single(static p => p.GetProperty("parameterName").GetString() == "endType");

        endType.GetProperty("enumOptions")
            .GetProperty(((int)EndType.HighLow).ToString(CultureInfo.InvariantCulture))
            .GetString().Should().Be(nameof(EndType.HighLow));
    }
}
