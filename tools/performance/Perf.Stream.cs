namespace Performance;

// STREAM-STYLE INDICATORS

[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class StreamIndicators
{
    private static readonly IReadOnlyList<Bar> q = Data.GetDefault();
    private static readonly IReadOnlyList<Bar> o = Data.GetCompare();
    private const int n = 14;

    private readonly BarHub barHub = new();      // prepopulated
    private readonly BarHub barHubOther = new(); // for correlation, beta, etc.

    /* SETUP/CLEANUP - runs before and after each.
     *
     * This Setup implies that each benchmark
     * will start with a prepopulated observable
     * BarHub provider.
     *
     * We do this because we want to measure
     * the performance of observer methods
     * without the overhead of the provider. */

    [GlobalSetup]
    public void Setup()
    {
        barHub.Add(q);
        barHubOther.Add(o);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        barHub.EndTransmission();
        barHub.Cache.Clear();

        barHubOther.EndTransmission();
        barHubOther.Cache.Clear();
    }

    // BENCHMARKS

    /* Parameter arguments should match the Perf.Series.cs equivalents */

    [Benchmark] public object AdlHub() => barHub.ToAdlHub().Results;
    [Benchmark] public object AdxHub() => barHub.ToAdxHub(n).Results;
    [Benchmark] public object AlligatorHub() => barHub.ToAlligatorHub().Results;
    [Benchmark] public object AlmaHub() => barHub.ToAlmaHub(9, 0.85, 6).Results;
    [Benchmark] public object AroonHub() => barHub.ToAroonHub().Results;
    [Benchmark] public object AtrHub() => barHub.ToAtrHub(n).Results;
    [Benchmark] public object AtrStopHub() => barHub.ToAtrStopHub().Results;
    [Benchmark] public object AwesomeHub() => barHub.ToAwesomeHub().Results;
    [Benchmark] public object BollingerBandsHub() => barHub.ToBollingerBandsHub(20, 2).Results;
    [Benchmark] public object BopHub() => barHub.ToBopHub(n).Results;
    [Benchmark] public object CciHub() => barHub.ToCciHub(n).Results;
    [Benchmark] public object ChaikinOscHub() => barHub.ToChaikinOscHub().Results;
    [Benchmark] public object ChandelierHub() => barHub.ToChandelierHub().Results;
    [Benchmark] public object ChopHub() => barHub.ToChopHub(n).Results;
    [Benchmark] public object CmfHub() => barHub.ToCmfHub(n).Results;
    [Benchmark] public object CmoHub() => barHub.ToCmoHub(n).Results;
    [Benchmark] public object ConnorsRsiHub() => barHub.ToConnorsRsiHub(3, 2, 100).Results;
    [Benchmark] public object DemaHub() => barHub.ToDemaHub(n).Results;
    [Benchmark] public object DojiHub() => barHub.ToDojiHub().Results;
    [Benchmark] public object DonchianHub() => barHub.ToDonchianHub(20).Results;
    [Benchmark] public object DpoHub() => barHub.ToDpoHub(n).Results;
    [Benchmark] public object DynamicHub() => barHub.ToDynamicHub(n).Results;
    [Benchmark] public object ElderRayHub() => barHub.ToElderRayHub(13).Results;
    [Benchmark] public object EmaHub() => barHub.ToEmaHub(20).Results;
    [Benchmark] public object EpmaHub() => barHub.ToEpmaHub(n).Results;
    [Benchmark] public object FisherTransformHub() => barHub.ToFisherTransformHub(10).Results;
    [Benchmark] public object FcbHub() => barHub.ToFcbHub(2).Results;
    [Benchmark] public object FractalHub() => barHub.ToFractalHub().Results;
    [Benchmark] public object ForceIndexHub() => barHub.ToForceIndexHub(2).Results;
    [Benchmark] public object GatorHub() => barHub.ToGatorHub().Results;
    [Benchmark] public object HmaHub() => barHub.ToHmaHub(n).Results;
    [Benchmark] public object HeikinAshiHub() => barHub.ToHeikinAshiHub().Results;
    [Benchmark] public object HtTrendlineHub() => barHub.ToHtTrendlineHub().Results;
    [Benchmark] public object HurstHub() => barHub.ToHurstHub(100).Results;
    [Benchmark] public object IchimokuHub() => barHub.ToIchimokuHub().Results;
    [Benchmark] public object KamaHub() => barHub.ToKamaHub(10, 2, 30).Results;
    [Benchmark] public object KvoHub() => barHub.ToKvoHub(34, 55, 13).Results;
    [Benchmark] public object KeltnerHub() => barHub.ToKeltnerHub(20, 2, 10).Results;
    [Benchmark] public object MacdHub() => barHub.ToMacdHub(12, 26, 9).Results;
    [Benchmark] public object MaEnvelopesHub() => barHub.ToMaEnvelopesHub(20, 2.5, MaType.SMA).Results;
    [Benchmark] public object MamaHub() => barHub.ToMamaHub(0.5, 0.05).Results;
    [Benchmark] public object MfiHub() => barHub.ToMfiHub(14).Results;
    [Benchmark] public object MarubozuHub() => barHub.ToMarubozuHub(95).Results;
    [Benchmark] public object ObvHub() => barHub.ToObvHub().Results;
    [Benchmark] public object ParabolicSarHub() => barHub.ToParabolicSarHub().Results;
    [Benchmark] public object PivotPointsHub() => barHub.ToPivotPointsHub(BarInterval.Month, PivotPointType.Standard).Results;
    [Benchmark] public object PivotsHub() => barHub.ToPivotsHub(2, 2, 20).Results;
    [Benchmark] public object PmoHub() => barHub.ToPmoHub(35, 20, 10).Results;
    [Benchmark] public object PvoHub() => barHub.ToPvoHub().Results;
    [Benchmark] public object BarHub() => barHub.ToBarHub().Results;
    [Benchmark] public object BarPartHub() => barHub.ToBarPartHub(CandlePart.OHL3).Results;
    [Benchmark] public object RenkoHub() => barHub.ToRenkoHub(2.5m).Results;
    [Benchmark] public object RocHub() => barHub.ToRocHub(20).Results;
    [Benchmark] public object RocWbHub() => barHub.ToRocWbHub(20, 5, 5).Results;
    [Benchmark] public object RollingPivotsHub() => barHub.ToRollingPivotsHub(20, 0, PivotPointType.Standard).Results;
    [Benchmark] public object RsiHub() => barHub.ToRsiHub(n).Results;
    [Benchmark] public object SlopeHub() => barHub.ToSlopeHub(n).Results;
    [Benchmark] public object SmaHub() => barHub.ToSmaHub(n).Results;
    [Benchmark] public object SmiHub() => barHub.ToSmiHub(13, 25, 2, 3).Results;
    [Benchmark] public object SmaAnalysisHub() => barHub.ToSmaAnalysisHub(n).Results;
    [Benchmark] public object SmmaHub() => barHub.ToSmmaHub(n).Results;
    [Benchmark] public object StarcBandsHub() => barHub.ToStarcBandsHub(5, 2, 10).Results;
    [Benchmark] public object StcHub() => barHub.ToStcHub(10, 23, 50).Results;
    [Benchmark] public object StdDevHub() => barHub.ToStdDevHub(n).Results;
    [Benchmark] public object StochHub() => barHub.ToStochHub(n, 3, 3).Results;
    [Benchmark] public object StochRsiHub() => barHub.ToStochRsiHub(n, n, 3, 1).Results;
    [Benchmark] public object SuperTrendHub() => barHub.ToSuperTrendHub(10, 3).Results;
    [Benchmark] public object T3Hub() => barHub.ToT3Hub(5, 0.7).Results;
    [Benchmark] public object TemaHub() => barHub.ToTemaHub(n).Results;
    [Benchmark] public object TrHub() => barHub.ToTrHub().Results;
    [Benchmark] public object TrixHub() => barHub.ToTrixHub(n).Results;
    [Benchmark] public object TsiHub() => barHub.ToTsiHub(25, 13, 7).Results;
    [Benchmark] public object TtmSqueezeHub() => barHub.ToTtmSqueezeHub(20, 2, 1.5).Results;
    [Benchmark] public object UlcerIndexHub() => barHub.ToUlcerIndexHub(n).Results;
    [Benchmark] public object UltimateHub() => barHub.ToUltimateHub(7, n, 28).Results;
    [Benchmark] public object VolatilityStopHub() => barHub.ToVolatilityStopHub(7, 3).Results;
    [Benchmark] public object VortexHub() => barHub.ToVortexHub(n).Results;
    [Benchmark] public object VwapHub() => barHub.ToVwapHub().Results;
    [Benchmark] public object VwmaHub() => barHub.ToVwmaHub(n).Results;
    [Benchmark] public object WilliamsRHub() => barHub.ToWilliamsRHub().Results;
    [Benchmark] public object WmaHub() => barHub.ToWmaHub(n).Results;
}
