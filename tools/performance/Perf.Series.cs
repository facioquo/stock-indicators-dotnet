namespace Performance;

// SERIES-STYLE INDICATORS

[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class SeriesIndicators
{
    private static readonly IReadOnlyList<Bar> q = Data.GetDefault();
    private static readonly IReadOnlyList<Bar> o = Data.GetCompare();
    private const int n = 14;

    /* Parameter arguments should match the Catalog default values */

    [Benchmark] public void ToAdlBatch() => q.ToAdl();
    [Benchmark] public void ToAdxBatch() => q.ToAdx(n);
    [Benchmark] public void ToAlligatorBatch() => q.ToAlligator();
    [Benchmark] public void ToAlmaBatch() => q.ToAlma(9, 0.85, 6);
    [Benchmark] public void ToAroonBatch() => q.ToAroon();
    [Benchmark] public void ToAtrBatch() => q.ToAtr(n);
    [Benchmark] public void ToAtrStopBatch() => q.ToAtrStop();
    [Benchmark] public void ToAwesomeBatch() => q.ToAwesome();
    [Benchmark] public void ToBetaBatch() => q.ToBeta(o, 20, BetaType.Standard);
    [Benchmark] public void ToBetaUpBatch() => q.ToBeta(o, 20, BetaType.Up);
    [Benchmark] public void ToBetaDownBatch() => q.ToBeta(o, 20, BetaType.Down);
    [Benchmark] public void ToBetaAllBatch() => q.ToBeta(o, 20, BetaType.All);
    [Benchmark] public void ToBollingerBandsBatch() => q.ToBollingerBands(20, 2);
    [Benchmark] public void ToBopBatch() => q.ToBop(n);
    [Benchmark] public void ToCciBatch() => q.ToCci(n);
    [Benchmark] public void ToChaikinOscBatch() => q.ToChaikinOsc();
    [Benchmark] public void ToChandelierBatch() => q.ToChandelier();
    [Benchmark] public void ToChopBatch() => q.ToChop(n);
    [Benchmark] public void ToCmfBatch() => q.ToCmf(n);
    [Benchmark] public void ToCmoBatch() => q.ToCmo(n);
    [Benchmark] public void ToConnorsRsiBatch() => q.ToConnorsRsi(3, 2, 100);
    [Benchmark] public void ToCorrelationBatch() => q.ToCorrelation(o, 20);
    [Benchmark] public void ToDemaBatch() => q.ToDema(n);
    [Benchmark] public void ToDojiBatch() => q.ToDoji();
    [Benchmark] public void ToDonchianBatch() => q.ToDonchian();
    [Benchmark] public void ToDpoBatch() => q.ToDpo(n);
    [Benchmark] public void ToDynamicBatch() => q.ToDynamic(n);
    [Benchmark] public void ToElderRayBatch() => q.ToElderRay(13);
    [Benchmark] public void ToEmaBatch() => q.ToEma(20);
    [Benchmark] public void ToEpmaBatch() => q.ToEpma(n);
    [Benchmark] public void ToFcbBatch() => q.ToFcb(2);
    [Benchmark] public void ToFisherTransformBatch() => q.ToFisherTransform(10);
    [Benchmark] public void ToForceIndexBatch() => q.ToForceIndex(2);
    [Benchmark] public void ToFractalBatch() => q.ToFractal();
    [Benchmark] public void ToGatorBatch() => q.ToGator();
    [Benchmark] public void ToHeikinAshiBatch() => q.ToHeikinAshi();
    [Benchmark] public void ToHmaBatch() => q.ToHma(n);
    [Benchmark] public void ToHtTrendlineBatch() => q.ToHtTrendline();
    [Benchmark] public void ToHurstBatch() => q.ToHurst(100);
    [Benchmark] public void ToIchimokuBatch() => q.ToIchimoku();
    [Benchmark] public void ToKamaBatch() => q.ToKama(10, 2, 30);
    [Benchmark] public void ToKeltnerBatch() => q.ToKeltner(20, 2, 10);
    [Benchmark] public void ToKvoBatch() => q.ToKvo(34, 55, 13);
    [Benchmark] public void ToMacdBatch() => q.ToMacd(12, 26, 9);
    [Benchmark] public void ToMaEnvelopesBatch() => q.ToMaEnvelopes(20, 2.5, MaType.SMA);
    [Benchmark] public void ToMamaBatch() => q.ToMama(0.5, 0.05);
    [Benchmark] public void ToMarubozuBatch() => q.ToMarubozu(95);
    [Benchmark] public void ToMfiBatch() => q.ToMfi(14);
    [Benchmark] public void ToObvBatch() => q.ToObv();
    [Benchmark] public void ToParabolicSarBatch() => q.ToParabolicSar();
    [Benchmark] public void ToPivotPointsBatch() => q.ToPivotPoints(BarInterval.Month, PivotPointType.Standard);
    [Benchmark] public void ToPivotsBatch() => q.ToPivots(2, 2, 20);
    [Benchmark] public void ToPmoBatch() => q.ToPmo(35, 20, 10);
    [Benchmark] public void ToPrsBatch() => q.ToPrs(o);
    [Benchmark] public void ToPvoBatch() => q.ToPvo();
    [Benchmark] public void ToBarPartBatch() => q.ToBarPart(CandlePart.OHL3);
    [Benchmark] public void ToRenkoBatch() => q.ToRenko(2.5m);
    [Benchmark] public void ToRenkoAtrBatch() => q.ToRenko(n);
    [Benchmark] public void ToRocBatch() => q.ToRoc(20);
    [Benchmark] public void ToRocWbBatch() => q.ToRocWb(20, 5, 5);
    [Benchmark] public void ToRollingPivotsBatch() => q.ToRollingPivots(20, 0, PivotPointType.Standard);
    [Benchmark] public void ToRsiBatch() => q.ToRsi(n);
    [Benchmark] public void ToSlopeBatch() => q.ToSlope(n);
    [Benchmark] public void ToSmaBatch() => q.ToSma(n);
    [Benchmark] public void ToSmaAnalysisBatch() => q.ToSmaAnalysis(n);
    [Benchmark] public void ToSmiBatch() => q.ToSmi(13, 25, 2, 3);
    [Benchmark] public void ToSmmaBatch() => q.ToSmma(n);
    [Benchmark] public void ToStarcBandsBatch() => q.ToStarcBands(5, 2, 10);
    [Benchmark] public void ToStcBatch() => q.ToStc(10, 23, 50);
    [Benchmark] public void ToStdDevBatch() => q.ToStdDev(n);
    [Benchmark] public void ToStdDevChannelsBatch() => q.ToStdDevChannels();
    [Benchmark] public void ToStochBatch() => q.ToStoch(14, 3, 3);
    [Benchmark] public void ToStochSmmaBatch() => q.ToStoch(n, 3, 3);
    [Benchmark] public void ToStochRsiBatch() => q.ToStochRsi(n, n, 3, 1);
    [Benchmark] public void ToSuperTrendBatch() => q.ToSuperTrend(10, 3);
    [Benchmark] public void ToT3Batch() => q.ToT3(5, 0.7);
    [Benchmark] public void ToTemaBatch() => q.ToTema(n);
    [Benchmark] public void ToTrBatch() => q.ToTr();
    [Benchmark] public void ToTrixBatch() => q.ToTrix(n);
    [Benchmark] public void ToTsiBatch() => q.ToTsi(25, 13, 7);
    [Benchmark] public void ToTtmSqueezeBatch() => q.ToTtmSqueeze(20, 2, 1.5);
    [Benchmark] public void ToUlcerIndexBatch() => q.ToUlcerIndex(n);
    [Benchmark] public void ToUltimateBatch() => q.ToUltimate(7, 14, 28);
    [Benchmark] public void ToVolatilityStopBatch() => q.ToVolatilityStop(7, 3);
    [Benchmark] public void ToVortexBatch() => q.ToVortex(n);
    [Benchmark] public void ToVwapBatch() => q.ToVwap();
    [Benchmark] public void ToVwmaBatch() => q.ToVwma(n);
    [Benchmark] public void ToWilliamsRBatch() => q.ToWilliamsR();
    [Benchmark] public void ToWmaBatch() => q.ToWma(n);
    [Benchmark] public void ToZigZagBatch() => q.ToZigZag();
}
