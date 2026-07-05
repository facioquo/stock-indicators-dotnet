namespace FacioQuo.Stock.Indicators;

// Test-only extension methods for StreamHub creation from IReadOnlyList<IBar>.
// These create standalone pre-populated hubs for testing convenience.
public static class TestExtensions
{
    public static AdlHub ToAdlHub(
        this IReadOnlyList<IBar> bars)
        => bars.ToBarHub().ToAdlHub();

    public static AdxHub ToAdxHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAdxHub(lookbackPeriods);
    }

    public static AlligatorHub ToAlligatorHub(
        this IReadOnlyList<IBar> bars,
        int jawPeriods = 13,
        int jawOffset = 8,
        int teethPeriods = 8,
        int teethOffset = 5,
        int lipsPeriods = 5,
        int lipsOffset = 3)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAlligatorHub(jawPeriods, jawOffset, teethPeriods, teethOffset, lipsPeriods, lipsOffset);
    }

    public static AlmaHub ToAlmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 9,
        double offset = 0.85,
        double sigma = 6)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAlmaHub(lookbackPeriods, offset, sigma);
    }

    public static AroonHub ToAroonHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 25)
        => bars.ToBarHub().ToAroonHub(lookbackPeriods);

    public static AtrHub ToAtrHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAtrHub(lookbackPeriods);
    }

    public static AtrStopHub ToAtrStopHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 21,
        double multiplier = 3,
        EndType endType = EndType.Close)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAtrStopHub(lookbackPeriods, multiplier, endType);
    }

    public static AwesomeHub ToAwesomeHub(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 5,
        int slowPeriods = 34)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToAwesomeHub(fastPeriods, slowPeriods);
    }

    public static BollingerBandsHub ToBollingerBandsHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20,
        double standardDeviations = 2)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToBollingerBandsHub(lookbackPeriods, standardDeviations);
    }

    public static BopHub ToBopHub(
        this IReadOnlyList<IBar> bars,
        int smoothPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToBopHub(smoothPeriods);
    }

    public static CciHub ToCciHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToCciHub(lookbackPeriods);
    }

    public static ChaikinOscHub ToChaikinOscHub(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 3,
        int slowPeriods = 10)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToChaikinOscHub(fastPeriods, slowPeriods);
    }

    public static ChandelierHub ToChandelierHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 22,
        double multiplier = 3,
        Direction type = Direction.Long)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToChandelierHub(lookbackPeriods, multiplier, type);
    }

    public static ChopHub ToChopHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToChopHub(lookbackPeriods);
    }

    public static CmfHub ToCmfHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToCmfHub(lookbackPeriods);
    }

    public static CmoHub ToCmoHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToCmoHub(lookbackPeriods);
    }

    public static ConnorsRsiHub ToConnorsRsiHub(
        this IReadOnlyList<IBar> bars,
        int rsiPeriods = 3,
        int streakPeriods = 2,
        int rankPeriods = 100)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToConnorsRsiHub(rsiPeriods, streakPeriods, rankPeriods);
    }

    public static DemaHub ToDemaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToDemaHub(lookbackPeriods);
    }

    public static DojiHub ToDojiHub(
        this IReadOnlyList<IBar> bars,
        double maxPriceChangePercent = 0.1)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToDojiHub(maxPriceChangePercent);
    }

    public static DonchianHub ToDonchianHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToDonchianHub(lookbackPeriods);
    }

    public static DpoHub ToDpoHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToDpoHub(lookbackPeriods);
    }

    public static DynamicHub ToDynamicHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods,
        double kFactor = 0.6)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToDynamicHub(lookbackPeriods, kFactor);
    }

    public static ElderRayHub ToElderRayHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 13)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToElderRayHub(lookbackPeriods);
    }

    public static EmaHub ToEmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToEmaHub(lookbackPeriods);

        // reminder: can't be self-ref 'bars.ToHub' syntax
    }

    public static EpmaHub ToEpmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToEpmaHub(lookbackPeriods);
    }

    public static FcbHub ToFcbHub(
        this IReadOnlyList<IBar> bars,
        int windowSpan = 2)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToFcbHub(windowSpan);
    }

    public static FisherTransformHub ToFisherTransformHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 10)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToFisherTransformHub(lookbackPeriods);
    }

    public static ForceIndexHub ToForceIndexHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 2)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToForceIndexHub(lookbackPeriods);
    }

    public static FractalHub ToFractalHub(
        this IReadOnlyList<IBar> bars,
        int windowSpan = 2,
        EndType endType = EndType.HighLow)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToFractalHub(windowSpan, endType);
    }

    public static FractalHub ToFractalHub(
        this IReadOnlyList<IBar> bars,
        int leftSpan,
        int rightSpan,
        EndType endType = EndType.HighLow)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToFractalHub(leftSpan, rightSpan, endType);
    }

    public static GatorHub ToGatorHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToGatorHub();
    }

    public static HeikinAshiHub ToHeikinAshiHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToHeikinAshiHub();
    }

    public static HmaHub ToHmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToHmaHub(lookbackPeriods);
    }

    public static HtTrendlineHub ToHtTrendlineHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToHtTrendlineHub();
    }

    public static HurstHub ToHurstHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 100)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToHurstHub(lookbackPeriods);
    }

    public static IchimokuHub ToIchimokuHub(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods = 9,
        int kijunPeriods = 26,
        int senkouBPeriods = 52)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods);
    }

    public static IchimokuHub ToIchimokuHub(
        this IReadOnlyList<IBar> bars,
        int tenkanPeriods,
        int kijunPeriods,
        int senkouBPeriods,
        int offsetPeriods)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToIchimokuHub(tenkanPeriods, kijunPeriods, senkouBPeriods, offsetPeriods);
    }

    public static KamaHub ToKamaHub(
        this IReadOnlyList<IBar> bars,
        int erPeriods = 10,
        int fastPeriods = 2,
        int slowPeriods = 30)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToKamaHub(erPeriods, fastPeriods, slowPeriods);
    }

    public static KeltnerHub ToKeltnerHub(
        this IReadOnlyList<IBar> bars,
        int emaPeriods = 20,
        double multiplier = 2,
        int atrPeriods = 10)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToKeltnerHub(emaPeriods, multiplier, atrPeriods);
    }

    public static KvoHub ToKvoHub(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 34,
        int slowPeriods = 55,
        int signalPeriods = 13)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToKvoHub(fastPeriods, slowPeriods, signalPeriods);
    }

    public static MacdHub ToMacdHub(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToMacdHub(fastPeriods, slowPeriods, signalPeriods);
    }

    public static MamaHub ToMamaHub(
        this IReadOnlyList<IBar> bars,
        double fastLimit = 0.5,
        double slowLimit = 0.05)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToMamaHub(fastLimit, slowLimit);
    }

    public static MarubozuHub ToMarubozuHub(
        this IReadOnlyList<IBar> bars,
        double minBodyPercent = 95)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToMarubozuHub(minBodyPercent);
    }

    public static MfiHub ToMfiHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToMfiHub(lookbackPeriods);
    }

    public static ObvHub ToObvHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToObvHub();
    }

    public static ParabolicSarHub ToParabolicSarHub(
        this IReadOnlyList<IBar> bars,
        double accelerationStep = 0.02,
        double maxAccelerationFactor = 0.2)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToParabolicSarHub(accelerationStep, maxAccelerationFactor);
    }

    public static ParabolicSarHub ToParabolicSarHub(
        this IReadOnlyList<IBar> bars,
        double accelerationStep,
        double maxAccelerationFactor,
        double initialFactor)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToParabolicSarHub(accelerationStep, maxAccelerationFactor, initialFactor);
    }

    public static PivotPointsHub ToPivotPointsHub(
        this IReadOnlyList<IBar> bars,
        BarInterval windowSize = BarInterval.Month,
        PivotPointType pointType = PivotPointType.Standard)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToPivotPointsHub(windowSize, pointType);
    }

    public static PivotsHub ToPivotsHub(
        this IReadOnlyList<IBar> bars,
        int leftSpan = 2,
        int rightSpan = 2,
        int maxTrendPeriods = 20,
        EndType endType = EndType.HighLow)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToPivotsHub(leftSpan, rightSpan, maxTrendPeriods, endType);
    }

    public static PmoHub ToPmoHub(
        this IReadOnlyList<IBar> bars,
        int timePeriods = 35,
        int smoothPeriods = 20,
        int signalPeriods = 10)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToPmoHub(timePeriods, smoothPeriods, signalPeriods);
    }

    public static PvoHub ToPvoHub(
        this IReadOnlyList<IBar> bars,
        int fastPeriods = 12,
        int slowPeriods = 26,
        int signalPeriods = 9)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToPvoHub(fastPeriods, slowPeriods, signalPeriods);
    }

    public static BarHub ToBarHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();  // cannot dogfood ToBarHub() here
        barHub.Add(bars);
        return barHub;
    }

    public static BarPartHub ToBarPartHub(
        this IReadOnlyList<IBar> bars,
        CandlePart candlePart)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToBarPartHub(candlePart);
    }

    public static RenkoHub ToRenkoHub(
        this IReadOnlyList<IBar> bars,
        decimal brickSize,
        EndType endType = EndType.Close)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToRenkoHub(brickSize, endType);
    }

    public static RocHub ToRocHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToRocHub(lookbackPeriods);
    }

    public static RocWbHub ToRocWbHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20,
        int emaPeriods = 5,
        int stdDevPeriods = 5)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToRocWbHub(lookbackPeriods, emaPeriods, stdDevPeriods);
    }

    public static RollingPivotsHub ToRollingPivotsHub(
        this IReadOnlyList<IBar> bars,
        int windowPeriods = 20,
        int offsetPeriods = 0,
        PivotPointType pointType = PivotPointType.Standard)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToRollingPivotsHub(windowPeriods, offsetPeriods, pointType);
    }

    public static RsiHub ToRsiHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToRsiHub(lookbackPeriods);
    }

    public static SlopeHub ToSlopeHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSlopeHub(lookbackPeriods);

        // reminder: can't be self-ref 'bars.ToHub' syntax
    }

    public static SmaAnalysisHub ToSmaAnalysisHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSmaAnalysisHub(lookbackPeriods);
    }

    public static SmaHub ToSmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSmaHub(lookbackPeriods);
    }

    public static SmiHub ToSmiHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 13,
        int firstSmoothPeriods = 25,
        int secondSmoothPeriods = 2,
        int signalPeriods = 3)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSmiHub(lookbackPeriods, firstSmoothPeriods, secondSmoothPeriods, signalPeriods);
    }

    public static SmmaHub ToSmmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSmmaHub(lookbackPeriods);
    }

    public static StcHub ToStcHub(
        this IReadOnlyList<IBar> bars,
        int cyclePeriods = 10,
        int fastPeriods = 23,
        int slowPeriods = 50)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToStcHub(cyclePeriods, fastPeriods, slowPeriods);
    }

    public static StarcBandsHub ToStarcBandsHub(
        this IReadOnlyList<IBar> bars,
        int smaPeriods = 5,
        double multiplier = 2,
        int atrPeriods = 10)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToStarcBandsHub(smaPeriods, multiplier, atrPeriods);
    }

    public static StdDevHub ToStdDevHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToStdDevHub(lookbackPeriods);
    }

    public static StochHub ToStochHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 3)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToStochHub(lookbackPeriods, signalPeriods, smoothPeriods);
    }

    public static StochRsiHub ToStochRsiHub(
        this IReadOnlyList<IBar> bars,
        int rsiPeriods = 14,
        int stochPeriods = 14,
        int signalPeriods = 3,
        int smoothPeriods = 1)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToStochRsiHub(rsiPeriods, stochPeriods, signalPeriods, smoothPeriods);
    }

    public static SuperTrendHub ToSuperTrendHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 10,
        double multiplier = 3)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToSuperTrendHub(lookbackPeriods, multiplier);
    }

    public static T3Hub ToT3Hub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 5,
        double volumeFactor = 0.7)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToT3Hub(lookbackPeriods, volumeFactor);
    }

    public static TemaHub ToTemaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 20)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToTemaHub(lookbackPeriods);
    }

    public static TrHub ToTrHub(
        this IReadOnlyList<IBar> bars)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToTrHub();
    }

    public static TrixHub ToTrixHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToTrixHub(lookbackPeriods);
    }

    public static TsiHub ToTsiHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 25,
        int smoothPeriods = 13,
        int signalPeriods = 7)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToTsiHub(lookbackPeriods, smoothPeriods, signalPeriods);
    }

    public static UlcerIndexHub ToUlcerIndexHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToUlcerIndexHub(lookbackPeriods);
    }

    public static UltimateHub ToUltimateHub(
        this IReadOnlyList<IBar> bars,
        int shortPeriods = 7,
        int middlePeriods = 14,
        int longPeriods = 28)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToUltimateHub(shortPeriods, middlePeriods, longPeriods);
    }

    public static VolatilityStopHub ToVolatilityStopHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 7,
        double multiplier = 3)
    {
        ArgumentNullException.ThrowIfNull(bars);
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToVolatilityStopHub(lookbackPeriods, multiplier);
    }

    public static VortexHub ToVortexHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToVortexHub(lookbackPeriods);
    }

    public static VwapHub ToVwapHub(
        this IReadOnlyList<IBar> bars,
        DateTime? startDate = null)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToVwapHub(startDate);
    }

    public static VwmaHub ToVwmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToVwmaHub(lookbackPeriods);
    }

    public static WilliamsRHub ToWilliamsRHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods = 14)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToWilliamsRHub(lookbackPeriods);
    }

    public static WmaHub ToWmaHub(
        this IReadOnlyList<IBar> bars,
        int lookbackPeriods)
    {
        BarHub barHub = new();
        barHub.Add(bars);
        return barHub.ToWmaHub(lookbackPeriods);
    }
}
