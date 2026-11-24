using System.Reflection;

namespace Catalogging;

/// <summary>
/// Pins every listing's numeric parameter bounds to the indicator's argument validation,
/// by calling each listing's method, in every style, at and just past each bound.
/// </summary>
/// <remarks>
/// A catalog bound must never advertise a value that throws. An integer minimum must also
/// equal the validation minimum, so one below it throws. A maximum is a recommended cap:
/// validation rarely has an upper limit, so only its acceptance is checked. A double
/// minimum is often an exclusive bound with no representable equivalent, so it too is only
/// checked for acceptance. A rejection counts only when it names the probed parameter, so
/// an incidental out-of-range index cannot pass for validation. Every other parameter keeps
/// its catalog default. A <c>CrossParameter</c> bound is checked only for rejection, so
/// a minimum there that is higher than validation allows is not detected by this test.
/// </remarks>
[TestClass]
public class CatalogParameterRangeTests : TestBase
{
    /// <summary>
    /// Bounds that are valid alone but conflict with another parameter's default, keyed
    /// <c>UIID.parameter.bound</c>. The catalog has no way to express the relation.
    /// </summary>
    private static readonly Dictionary<string, string> CrossParameter = new(StringComparer.Ordinal) {
        ["ALLIGATOR.jawPeriods.min"] = "jawPeriods > teethPeriods (8)",
        ["ALLIGATOR.teethPeriods.min"] = "teethPeriods > lipsPeriods (5)",
        ["ALLIGATOR.teethPeriods.max"] = "jawPeriods (13) > teethPeriods",
        ["ALLIGATOR.teethOffset.max"] = "jaw + jawOffset (21) > teeth + teethOffset",
        ["ALLIGATOR.lipsPeriods.max"] = "teethPeriods (8) > lipsPeriods",
        ["ALLIGATOR.lipsOffset.max"] = "teeth + teethOffset (13) > lips + lipsOffset",
        ["AWESOME.fastPeriods.max"] = "slowPeriods (34) > fastPeriods",
        ["AWESOME.slowPeriods.min"] = "slowPeriods > fastPeriods (5)",
        ["CHAIKIN-OSC.fastPeriods.max"] = "slowPeriods (10) > fastPeriods",
        ["CHAIKIN-OSC.slowPeriods.min"] = "slowPeriods > fastPeriods (3)",
        ["ICHIMOKU.kijunPeriods.max"] = "senkouBPeriods (52) > kijunPeriods",
        ["ICHIMOKU.senkouBPeriods.min"] = "senkouBPeriods > kijunPeriods (26)",
        ["KAMA.fastPeriods.max"] = "slowPeriods (30) > fastPeriods",
        ["KAMA.slowPeriods.min"] = "slowPeriods > fastPeriods (2)",
        ["KVO.fastPeriods.max"] = "slowPeriods (55) > fastPeriods",
        ["KVO.slowPeriods.min"] = "slowPeriods > fastPeriods (34)",
        ["MACD.fastPeriods.max"] = "slowPeriods (26) > fastPeriods",
        ["MACD.slowPeriods.min"] = "slowPeriods > fastPeriods (12)",
        ["MAMA.fastLimit.min"] = "fastLimit > slowLimit (0.05)",
        ["MAMA.slowLimit.max"] = "fastLimit (0.5) > slowLimit",
        ["PIVOTS.leftSpan.max"] = "maxTrendPeriods (20) > leftSpan",
        ["PIVOTS.maxTrendPeriods.min"] = "maxTrendPeriods > leftSpan (2)",
        ["PVO.fastPeriods.max"] = "slowPeriods (26) > fastPeriods",
        ["PVO.slowPeriods.min"] = "slowPeriods > fastPeriods (12)",
        ["ROC-WB.lookbackPeriods.min"] = "stdDevPeriods (5) <= lookbackPeriods",
        ["ROC-WB.stdDevPeriods.max"] = "stdDevPeriods <= lookbackPeriods (20)",
        ["STC.fastPeriods.max"] = "slowPeriods (50) > fastPeriods",
        ["STC.slowPeriods.min"] = "slowPeriods > fastPeriods (23)",
        ["UO.shortPeriods.max"] = "shortPeriods < middlePeriods (14)",
        ["UO.middlePeriods.min"] = "shortPeriods (7) < middlePeriods",
        ["UO.middlePeriods.max"] = "middlePeriods < longPeriods (28)",
        ["UO.longPeriods.min"] = "middlePeriods (14) < longPeriods",
    };

    [TestMethod]
    public void NumericBounds_MatchArgumentValidation()
    {
        List<string> failures = [];
        HashSet<string> crossSeen = [with(StringComparer.Ordinal)];
        HashSet<Style> stylesProbed = [];

        foreach (IndicatorListing listing in Catalog.Get())
        {
            foreach (IndicatorParam param in listing.Parameters ?? [])
            {
                bool isInt = param.DataType == "Int32";
                if (!isInt && param.DataType is not "Double" and not "Decimal")
                {
                    continue;
                }

                stylesProbed.Add(listing.Style);

                string key = $"{listing.Uiid}.{param.ParameterName}";

                Expect(listing, param, isInt, $"{key}.min", param.Minimum, accepted: true);
                Expect(listing, param, isInt, $"{key}.max", param.Maximum, accepted: true);

                if (isInt && param.Minimum is double min)
                {
                    Expect(listing, param, isInt, $"{key}.below-min", min - 1, accepted: false);
                }
            }
        }

        failures.AddRange(Enum.GetValues<Style>()
            .Where(st => !stylesProbed.Contains(st))
            .Select(static st => $"{st}: no listing in this style was probed"));

        failures.AddRange(CrossParameter.Keys
            .Where(k => !crossSeen.Contains(k))
            .Select(static k => $"{k}: exemption no longer needed; remove it"));

        if (failures.Count > 0)
        {
            Assert.Fail($"Catalog bounds disagree with validation:\n{string.Join("\n", failures)}");
        }

        void Expect(IndicatorListing listing, IndicatorParam param, bool isInt, string key, double? value, bool accepted)
        {
            if (value is not double v)
            {
                return;
            }

            object arg = isInt ? (int)v
                : param.DataType == "Decimal" ? (decimal)v
                : (object)v;
            ArgumentOutOfRangeException rejection;

            try
            {
                rejection = Rejection(listing, param.ParameterName, arg);
            }
            catch (TargetInvocationException ex)
            {
                // any failure other than argument validation is a defect at an advertised value
                failures.Add($"{key} ({listing.Style}) = {arg}: {ex.InnerException?.GetType().Name}");
                return;
            }

            // a cross-parameter relation may be reported against either parameter
            if (CrossParameter.TryGetValue(key, out string _))
            {
                if (rejection is not null)
                {
                    crossSeen.Add(key);
                }

                return;
            }

            // an out-of-range index or another parameter's check is not this parameter's validation
            if (rejection is not null && rejection.ParamName != param.ParameterName)
            {
                failures.Add($"{key} ({listing.Style}) = {arg}: rejected for '{rejection.ParamName}', not '{param.ParameterName}'");
                return;
            }

            bool threw = rejection is not null;

            if (threw == accepted)
            {
                failures.Add(accepted
                    ? $"{key} ({listing.Style}) = {arg}: catalog advertises a value validation rejects"
                    : $"{key} ({listing.Style}) = {arg}: validation accepts a value below the catalog minimum");
            }
        }
    }

    /// <summary>
    /// Runs the listing with one parameter overridden; series inputs reuse the test bars.
    /// </summary>
    /// <returns>The argument validation exception, or <c>null</c> when the call succeeded.</returns>
    private static ArgumentOutOfRangeException Rejection(IndicatorListing listing, string parameterName, object value)
    {
        Dictionary<string, object> args = new(StringComparer.Ordinal) { [parameterName] = value };

        try
        {
            foreach (IndicatorParam series in (listing.Parameters ?? [])
                .Where(static p => p.DataType == IndicatorParam.SeriesDataType))
            {
                args[series.ParameterName] = Bars.Cast<IReusable>().ToList();
            }

            _ = ListingExecutor.Execute<object>(Bars, listing, args);
            return null;
        }
        catch (TargetInvocationException ex) when (ex.InnerException is ArgumentOutOfRangeException inner)
        {
            return inner;
        }
        catch (ArgumentOutOfRangeException ex)
        {
            return ex;
        }
    }
}
