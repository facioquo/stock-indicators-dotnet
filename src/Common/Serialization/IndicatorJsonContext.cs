using System.Diagnostics.CodeAnalysis;

namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Build-time System.Text.Json metadata for every indicator result list a Series method returns,
/// so trimmed and Native AOT apps serialize results without reflection.
/// </summary>
/// <remarks>
/// <para>
/// Pass a type from <see cref="Default"/> to the serializer. The output matches reflection-based
/// <see cref="System.Text.Json.JsonSerializer"/> with default options:
/// </para>
/// <code>
/// IReadOnlyList&lt;EmaResult&gt; results = bars.ToEma(20);
/// string json = JsonSerializer.Serialize(results, IndicatorJsonContext.Default.IReadOnlyListEmaResult);
/// </code>
/// <para>
/// For other settings, such as camel-case names, construct the context with your own
/// <see cref="System.Text.Json.JsonSerializerOptions"/>, or add <see cref="Default"/> to the
/// options' <c>TypeInfoResolverChain</c>. Apps that never reference this type trim it away.
/// </para>
/// </remarks>
[JsonSerializable(typeof(IReadOnlyList<AdlResult>))]
[JsonSerializable(typeof(IReadOnlyList<AdxResult>))]
[JsonSerializable(typeof(IReadOnlyList<AlligatorResult>))]
[JsonSerializable(typeof(IReadOnlyList<AlmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<AroonResult>))]
[JsonSerializable(typeof(IReadOnlyList<AtrResult>))]
[JsonSerializable(typeof(IReadOnlyList<AtrStopResult>))]
[JsonSerializable(typeof(IReadOnlyList<AwesomeResult>))]
[JsonSerializable(typeof(IReadOnlyList<BetaResult>))]
[JsonSerializable(typeof(IReadOnlyList<BollingerBandsResult>))]
[JsonSerializable(typeof(IReadOnlyList<BopResult>))]
[JsonSerializable(typeof(IReadOnlyList<CciResult>))]
[JsonSerializable(typeof(IReadOnlyList<CandleProperties>))]
[JsonSerializable(typeof(IReadOnlyList<CandleResult>))]
[JsonSerializable(typeof(IReadOnlyList<ChaikinOscResult>))]
[JsonSerializable(typeof(IReadOnlyList<ChandelierResult>))]
[JsonSerializable(typeof(IReadOnlyList<ChopResult>))]
[JsonSerializable(typeof(IReadOnlyList<CmfResult>))]
[JsonSerializable(typeof(IReadOnlyList<CmoResult>))]
[JsonSerializable(typeof(IReadOnlyList<ConnorsRsiResult>))]
[JsonSerializable(typeof(IReadOnlyList<CorrResult>))]
[JsonSerializable(typeof(IReadOnlyList<DemaResult>))]
[JsonSerializable(typeof(IReadOnlyList<DonchianResult>))]
[JsonSerializable(typeof(IReadOnlyList<DpoResult>))]
[JsonSerializable(typeof(IReadOnlyList<DynamicResult>))]
[JsonSerializable(typeof(IReadOnlyList<ElderRayResult>))]
[JsonSerializable(typeof(IReadOnlyList<EmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<EpmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<FcbResult>))]
[JsonSerializable(typeof(IReadOnlyList<FisherTransformResult>))]
[JsonSerializable(typeof(IReadOnlyList<ForceIndexResult>))]
[JsonSerializable(typeof(IReadOnlyList<FractalResult>))]
[JsonSerializable(typeof(IReadOnlyList<GatorResult>))]
[JsonSerializable(typeof(IReadOnlyList<HeikinAshiResult>))]
[JsonSerializable(typeof(IReadOnlyList<HmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<HtlResult>))]
[JsonSerializable(typeof(IReadOnlyList<HurstResult>))]
[JsonSerializable(typeof(IReadOnlyList<IchimokuResult>))]
[JsonSerializable(typeof(IReadOnlyList<KamaResult>))]
[JsonSerializable(typeof(IReadOnlyList<KeltnerResult>))]
[JsonSerializable(typeof(IReadOnlyList<KvoResult>))]
[JsonSerializable(typeof(IReadOnlyList<MaEnvelopeResult>))]
[JsonSerializable(typeof(IReadOnlyList<MacdResult>))]
[JsonSerializable(typeof(IReadOnlyList<MamaResult>))]
[JsonSerializable(typeof(IReadOnlyList<MathResult>))]
[JsonSerializable(typeof(IReadOnlyList<MfiResult>))]
[JsonSerializable(typeof(IReadOnlyList<ObvResult>))]
[JsonSerializable(typeof(IReadOnlyList<ParabolicSarResult>))]
[JsonSerializable(typeof(IReadOnlyList<PivotPointsResult>))]
[JsonSerializable(typeof(IReadOnlyList<PivotsResult>))]
[JsonSerializable(typeof(IReadOnlyList<PmoResult>))]
[JsonSerializable(typeof(IReadOnlyList<PrsResult>))]
[JsonSerializable(typeof(IReadOnlyList<PvoResult>))]
[JsonSerializable(typeof(IReadOnlyList<RenkoResult>))]
[JsonSerializable(typeof(IReadOnlyList<RocResult>))]
[JsonSerializable(typeof(IReadOnlyList<RocWbResult>))]
[JsonSerializable(typeof(IReadOnlyList<RollingPivotsResult>))]
[JsonSerializable(typeof(IReadOnlyList<RsiResult>))]
[JsonSerializable(typeof(IReadOnlyList<SlopeResult>))]
[JsonSerializable(typeof(IReadOnlyList<SmaAnalysisResult>))]
[JsonSerializable(typeof(IReadOnlyList<SmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<SmiResult>))]
[JsonSerializable(typeof(IReadOnlyList<SmmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<StarcBandsResult>))]
[JsonSerializable(typeof(IReadOnlyList<StcResult>))]
[JsonSerializable(typeof(IReadOnlyList<StdDevChannelsResult>))]
[JsonSerializable(typeof(IReadOnlyList<StdDevResult>))]
[JsonSerializable(typeof(IReadOnlyList<StochResult>))]
[JsonSerializable(typeof(IReadOnlyList<StochRsiResult>))]
[JsonSerializable(typeof(IReadOnlyList<SuperTrendResult>))]
[JsonSerializable(typeof(IReadOnlyList<T3Result>))]
[JsonSerializable(typeof(IReadOnlyList<TemaResult>))]
[JsonSerializable(typeof(IReadOnlyList<TimeValue>))]
[JsonSerializable(typeof(IReadOnlyList<TrResult>))]
[JsonSerializable(typeof(IReadOnlyList<TrixResult>))]
[JsonSerializable(typeof(IReadOnlyList<TsiResult>))]
[JsonSerializable(typeof(IReadOnlyList<UlcerIndexResult>))]
[JsonSerializable(typeof(IReadOnlyList<UltimateResult>))]
[JsonSerializable(typeof(IReadOnlyList<VolatilityStopResult>))]
[JsonSerializable(typeof(IReadOnlyList<VortexResult>))]
[JsonSerializable(typeof(IReadOnlyList<VwapResult>))]
[JsonSerializable(typeof(IReadOnlyList<VwmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<WilliamsResult>))]
[JsonSerializable(typeof(IReadOnlyList<WmaResult>))]
[JsonSerializable(typeof(IReadOnlyList<ZigZagResult>))]
// the generator emits ~15k lines of metadata into this class; the tests exercise what callers use
[ExcludeFromCodeCoverage]
public sealed partial class IndicatorJsonContext : JsonSerializerContext;
