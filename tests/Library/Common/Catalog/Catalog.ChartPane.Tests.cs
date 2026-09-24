using System.Collections;
using System.Globalization;
using System.Reflection;

namespace Catalogging;

/// <summary>
/// Catalog chart-pane tests asserting that each result's <see cref="IndicatorResult.ChartPane"/>
/// matches what the result is:
/// - a result is uncharted exactly when its record property is not numeric
/// - a result on the price pane is at price level, and a result in any other pane is not
/// </summary>
/// <remarks>
/// A catalog-driven chart places each result by this value alone, so a wrong pane draws an
/// oscillator along the bottom of the price axis or a price level in a pane of its own.
/// </remarks>
[TestClass]
public class CatalogChartPaneTests : TestBase
{
    // results whose median lands near price level on the test data without being a price
    private static readonly Dictionary<(string Uiid, string DataName), string> ScaleCoincidences = new() {
        [("CORR", "VarianceB")] = "a variance is in squared price units",
        [("SLOPE", "Intercept")] = "the regression extrapolated to the first bar of the series, not a price at any bar"
    };

    [TestMethod]
    public void UnchartedExactlyWhenResultIsNotNumeric()
    {
        List<string> violations = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            Type resultType = CatalogReflection.GetOverloads(listing.MethodName)
                .Select(CatalogReflection.GetResultType)
                .FirstOrDefault(static t => t is not null);

            if (resultType is null)
            {
                continue; // reported by EveryListingBindsToAnIndicatorMethod
            }

            foreach (IndicatorResult result in listing.Results)
            {
                PropertyInfo property = resultType.GetProperty(result.DataName);

                if (property is null)
                {
                    continue; // reported by EveryResultDataNameExistsOnResultRecord
                }

                bool numeric = IsNumeric(property.PropertyType);

                if (numeric == result.ChartPane is null)
                {
                    violations.Add(
                        $"{CatalogReflection.Describe(listing)}: '{result.DataName}' is "
                      + $"{property.PropertyType.Name} but has ChartPane "
                      + $"'{result.ChartPane ?? "null"}'");
                }
            }
        }

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "a numeric result must name the pane it draws in, and a flag or pattern code has no axis to draw on");
    }

    [TestMethod]
    public void PricePaneExactlyWhenResultIsAtPriceLevel()
    {
        double closeMedian = Median(Bars.Select(static b => (double)b.Close));
        List<string> violations = [];
        int checkedResults = 0;

        foreach (IndicatorListing listing in Catalog.Get(Style.Series))
        {
            Type resultType = CatalogReflection.GetOverloads(listing.MethodName)
                .Select(CatalogReflection.GetResultType)
                .FirstOrDefault(static t => t is not null);

            if (resultType is null)
            {
                continue; // reported by EveryListingBindsToAnIndicatorMethod
            }

            IList rows = Execute(listing, resultType);

            foreach (IndicatorResult result in listing.Results.Where(static r => r.ChartPane is not null))
            {
                PropertyInfo property = resultType.GetProperty(result.DataName);

                if (property is null)
                {
                    continue; // reported by EveryResultDataNameExistsOnResultRecord
                }

                double[] values = rows.Cast<object>()
                    .Select(row => property.GetValue(row))
                    .Where(static v => v is not null)
                    .Select(static v => Convert.ToDouble(v, CultureInfo.InvariantCulture))
                    .Where(double.IsFinite)
                    .ToArray();

                if (values.Length == 0)
                {
                    continue;
                }

                checkedResults++;

                double ratio = Median(values) / closeMedian;
                bool atPriceLevel = ratio is >= 0.5 and <= 1.5;
                bool onPricePane = result.ChartPane == IndicatorResult.PricePane;

                bool excused = ScaleCoincidences.ContainsKey((listing.Uiid, result.DataName));

                if (atPriceLevel != onPricePane && !excused)
                {
                    violations.Add(
                        $"{CatalogReflection.Describe(listing)}: '{result.DataName}' has ChartPane "
                      + $"'{result.ChartPane}' but its median is {ratio:0.###}x the median close");
                }

                if (excused && (onPricePane || !atPriceLevel))
                {
                    violations.Add(
                        $"{CatalogReflection.Describe(listing)}: '{result.DataName}' is listed as a "
                      + "scale coincidence but no longer is one; remove it from the list");
                }
            }
        }

        checkedResults.Should().BePositive();

        string.Join(Environment.NewLine, violations).Should().BeEmpty(
            "a price-pane result must share the price axis's scale and any other pane's results must not");
    }

    private IList Execute(IndicatorListing listing, Type resultType)
    {
        ListingExecutionBuilder builder = new(listing);

        // the first series input is the source; any further one is the comparison
        string[] seriesParameters = (listing.Parameters ?? [])
            .Where(static p => p.DataType == IndicatorParam.SeriesDataType)
            .Select(static p => p.ParameterName)
            .ToArray();

        for (int i = 0; i < seriesParameters.Length; i++)
        {
            builder = builder.WithParamValue(seriesParameters[i], i == 0 ? Bars : OtherBars);
        }

        builder = builder.FromSource((IEnumerable<IBar>)Bars);

        return (IList)typeof(ListingExecutionBuilder)
            .GetMethod(nameof(ListingExecutionBuilder.Execute))!
            .MakeGenericMethod(resultType)
            .Invoke(builder, null)!;
    }

    private static bool IsNumeric(Type type)
    {
        Type underlying = Nullable.GetUnderlyingType(type) ?? type;
        return underlying == typeof(double) || underlying == typeof(decimal)
            || underlying == typeof(float) || underlying == typeof(int) || underlying == typeof(long);
    }

    private static double Median(IEnumerable<double> values)
    {
        double[] sorted = [.. values.Order()];
        int mid = sorted.Length / 2;
        return sorted.Length % 2 == 0 ? (sorted[mid - 1] + sorted[mid]) / 2 : sorted[mid];
    }
}
