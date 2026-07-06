namespace FacioQuo.Stock.Indicators;

/// <summary>
/// Provides a catalog of all registered indicators.
/// </summary>
/// <remarks>The <see cref="Catalog"/> class maintains a collection of indicators that can be accessed
/// through the <see cref="Listings"/> property. This class is static and cannot be instantiated.</remarks>
public static partial class Catalog
{
    private static readonly List<IndicatorListing> _listings = new();
    private static readonly object _lock = new();
    private static bool _initialized;

    /// <summary>
    /// Catalog of all indicators
    /// </summary>
    internal static IReadOnlyList<IndicatorListing> Listings
    {
        get {
            EnsureCatalogInitialized();
            return _listings;
        }
    }

    /// <summary>
    /// Clear the catalog (for testing purposes)
    /// </summary>
    internal static void ClearCatalog()
    {
        lock (_lock)
        {
            _listings.Clear();
            _initialized = false;
        }
    }

    /// <summary>
    /// Initialize the catalog with all available indicator listings
    /// </summary>
    private static void EnsureCatalogInitialized()
    {
        if (!_initialized)
        {
            lock (_lock)
            {
                if (!_initialized)
                {
                    PopulateCatalog();
                    _initialized = true;
                }
            }
        }
    }

    /// <summary>
    /// Populate the catalog with all indicator listings
    /// </summary>
    private static void PopulateCatalog()
    {
        // ADL (Accumulation Distribution Line)
        _listings.Add(Adl.BufferListing);
        _listings.Add(Adl.SeriesListing);
        _listings.Add(Adl.StreamListing);

        // ADX (Average Directional Index)
        _listings.Add(Adx.BufferListing);
        _listings.Add(Adx.SeriesListing);
        _listings.Add(Adx.StreamListing);

        // Alligator
        _listings.Add(Alligator.BufferListing);
        _listings.Add(Alligator.SeriesListing);
        _listings.Add(Alligator.StreamListing);

        // ALMA (Arnaud Legoux Moving Average)
        _listings.Add(Alma.BufferListing);
        _listings.Add(Alma.SeriesListing);
        _listings.Add(Alma.StreamListing);

        // Aroon
        _listings.Add(Aroon.BufferListing);
        _listings.Add(Aroon.SeriesListing);
        _listings.Add(Aroon.StreamListing);

        // ATR (Average True Range)
        _listings.Add(Atr.BufferListing);
        _listings.Add(Atr.SeriesListing);
        _listings.Add(Atr.StreamListing);

        // ATR Stop
        _listings.Add(AtrStop.BufferListing);
        _listings.Add(AtrStop.SeriesListing);
        _listings.Add(AtrStop.StreamListing);

        // Awesome Oscillator
        _listings.Add(Awesome.BufferListing);
        _listings.Add(Awesome.SeriesListing);
        _listings.Add(Awesome.StreamListing);

        // Beta
        _listings.Add(Beta.SeriesListing);

        // Bollinger Bands
        _listings.Add(BollingerBands.BufferListing);
        _listings.Add(BollingerBands.SeriesListing);
        _listings.Add(BollingerBands.StreamListing);

        // BOP (Balance of Power)
        _listings.Add(Bop.BufferListing);
        _listings.Add(Bop.SeriesListing);
        _listings.Add(Bop.StreamListing);

        // CCI (Commodity Channel Index)
        _listings.Add(Cci.BufferListing);
        _listings.Add(Cci.SeriesListing);
        _listings.Add(Cci.StreamListing);

        // Chaikin Oscillator
        _listings.Add(ChaikinOsc.BufferListing);
        _listings.Add(ChaikinOsc.SeriesListing);
        _listings.Add(ChaikinOsc.StreamListing);

        // Chandelier Exit
        _listings.Add(Chandelier.BufferListing);
        _listings.Add(Chandelier.SeriesListing);
        _listings.Add(Chandelier.StreamListing);

        // CHOP (Choppiness Index)
        _listings.Add(Chop.BufferListing);
        _listings.Add(Chop.SeriesListing);
        _listings.Add(Chop.StreamListing);

        // CMF (Chaikin Money Flow)
        _listings.Add(Cmf.BufferListing);
        _listings.Add(Cmf.SeriesListing);
        _listings.Add(Cmf.StreamListing);

        // CMO (Chande Momentum Oscillator)
        _listings.Add(Cmo.BufferListing);
        _listings.Add(Cmo.SeriesListing);
        _listings.Add(Cmo.StreamListing);

        // ConnorsRSI
        _listings.Add(ConnorsRsi.SeriesListing);
        _listings.Add(ConnorsRsi.BufferListing);
        _listings.Add(ConnorsRsi.StreamListing);

        // Correlation
        _listings.Add(Correlation.SeriesListing);

        // DEMA (Double Exponential Moving Average)
        _listings.Add(Dema.BufferListing);
        _listings.Add(Dema.SeriesListing);
        _listings.Add(Dema.StreamListing);

        // Doji
        _listings.Add(Doji.BufferListing);
        _listings.Add(Doji.SeriesListing);
        _listings.Add(Doji.StreamListing);

        // Donchian Channels
        _listings.Add(Donchian.SeriesListing);
        _listings.Add(Donchian.StreamListing);
        _listings.Add(Donchian.BufferListing);

        // DPO (Detrended Price Oscillator)
        _listings.Add(Dpo.BufferListing);
        _listings.Add(Dpo.SeriesListing);
        _listings.Add(Dpo.StreamListing);

        // Elder Ray
        _listings.Add(ElderRay.BufferListing);
        _listings.Add(ElderRay.SeriesListing);
        _listings.Add(ElderRay.StreamListing);

        // EMA (Exponential Moving Average)
        _listings.Add(Ema.BufferListing);
        _listings.Add(Ema.SeriesListing);
        _listings.Add(Ema.StreamListing);

        // EPMA (Endpoint Moving Average)
        _listings.Add(Epma.BufferListing);
        _listings.Add(Epma.SeriesListing);
        _listings.Add(Epma.StreamListing);

        // FCB (Fractal Chaos Bands)
        _listings.Add(Fcb.BufferListing);
        _listings.Add(Fcb.SeriesListing);
        _listings.Add(Fcb.StreamListing);

        // Fisher Transform
        _listings.Add(FisherTransform.SeriesListing);
        _listings.Add(FisherTransform.StreamListing);
        _listings.Add(FisherTransform.BufferListing);

        // Force Index
        _listings.Add(ForceIndex.BufferListing);
        _listings.Add(ForceIndex.SeriesListing);
        _listings.Add(ForceIndex.StreamListing);

        // Fractal
        _listings.Add(Fractal.BufferListing);
        _listings.Add(Fractal.SeriesListing);
        _listings.Add(Fractal.StreamListing);

        // Gator Oscillator
        _listings.Add(Gator.BufferListing);
        _listings.Add(Gator.SeriesListing);
        _listings.Add(Gator.StreamListing);

        // Heikin Ashi
        _listings.Add(HeikinAshi.BufferListing);
        _listings.Add(HeikinAshi.SeriesListing);
        _listings.Add(HeikinAshi.StreamListing);

        // HMA (Hull Moving Average)
        _listings.Add(Hma.BufferListing);
        _listings.Add(Hma.SeriesListing);
        _listings.Add(Hma.StreamListing);

        // HT Trendline (Hilbert Transform)
        _listings.Add(HtTrendline.BufferListing);
        _listings.Add(HtTrendline.SeriesListing);
        _listings.Add(HtTrendline.StreamListing);

        // Hurst Exponent
        _listings.Add(Hurst.SeriesListing);
        _listings.Add(Hurst.BufferListing);
        _listings.Add(Hurst.StreamListing);

        // Ichimoku Cloud
        _listings.Add(Ichimoku.BufferListing);
        _listings.Add(Ichimoku.SeriesListing);
        _listings.Add(Ichimoku.StreamListing);

        // KAMA (Kaufman Adaptive Moving Average)
        _listings.Add(Kama.BufferListing);
        _listings.Add(Kama.SeriesListing);
        _listings.Add(Kama.StreamListing);

        // Keltner Channels
        _listings.Add(Keltner.BufferListing);
        _listings.Add(Keltner.SeriesListing);
        _listings.Add(Keltner.StreamListing);

        // KVO (Klinger Volume Oscillator)
        _listings.Add(Kvo.BufferListing);
        _listings.Add(Kvo.SeriesListing);
        _listings.Add(Kvo.StreamListing);

        // MA Envelopes (Moving Average Envelopes)
        _listings.Add(MaEnvelopes.SeriesListing);
        _listings.Add(MaEnvelopes.BufferListing);
        _listings.Add(MaEnvelopes.StreamListing);

        // MACD (Moving Average Convergence Divergence)
        _listings.Add(Macd.BufferListing);
        _listings.Add(Macd.SeriesListing);
        _listings.Add(Macd.StreamListing);

        // MAMA (MESA Adaptive Moving Average)
        _listings.Add(Mama.BufferListing);
        _listings.Add(Mama.SeriesListing);
        _listings.Add(Mama.StreamListing);

        // Marubozu
        _listings.Add(Marubozu.BufferListing);
        _listings.Add(Marubozu.SeriesListing);
        _listings.Add(Marubozu.StreamListing);

        // MFI (Money Flow Index)
        _listings.Add(Mfi.BufferListing);
        _listings.Add(Mfi.SeriesListing);
        _listings.Add(Mfi.StreamListing);

        // McGinley Dynamic
        _listings.Add(MgDynamic.BufferListing);
        _listings.Add(MgDynamic.SeriesListing);
        _listings.Add(MgDynamic.StreamListing);

        // OBV (On Balance Volume)
        _listings.Add(Obv.BufferListing);
        _listings.Add(Obv.SeriesListing);
        _listings.Add(Obv.StreamListing);

        // Parabolic SAR
        _listings.Add(ParabolicSar.BufferListing);
        _listings.Add(ParabolicSar.SeriesListing);
        _listings.Add(ParabolicSar.StreamListing);

        // Pivot Points
        _listings.Add(PivotPoints.SeriesListing);
        _listings.Add(PivotPoints.BufferListing);
        _listings.Add(PivotPoints.StreamListing);

        // Pivots
        _listings.Add(Pivots.SeriesListing);
        _listings.Add(Pivots.BufferListing);
        _listings.Add(Pivots.StreamListing);

        // PMO (Price Momentum Oscillator)
        _listings.Add(Pmo.BufferListing);
        _listings.Add(Pmo.SeriesListing);
        _listings.Add(Pmo.StreamListing);

        // PRS (Price Relative Strength)
        _listings.Add(Prs.SeriesListing);

        // PVO (Price Volume Oscillator)
        _listings.Add(Pvo.BufferListing);
        _listings.Add(Pvo.SeriesListing);
        _listings.Add(Pvo.StreamListing);

        // Bar Part
        _listings.Add(BarParts.BufferListing);
        _listings.Add(BarParts.SeriesListing);
        _listings.Add(BarParts.StreamListing);

        // Renko
        _listings.Add(Renko.SeriesListing);
        _listings.Add(Renko.StreamListing);
        _listings.Add(Renko.BufferListing);

        // Renko ATR
        _listings.Add(RenkoAtr.SeriesListing);

        // ROC (Rate of Change)
        _listings.Add(Roc.BufferListing);
        _listings.Add(Roc.SeriesListing);
        _listings.Add(Roc.StreamListing);

        // ROC with Bands
        _listings.Add(RocWb.SeriesListing);
        _listings.Add(RocWb.StreamListing);
        _listings.Add(RocWb.BufferListing);

        // Rolling Pivots
        _listings.Add(RollingPivots.BufferListing);
        _listings.Add(RollingPivots.SeriesListing);
        _listings.Add(RollingPivots.StreamListing);

        // RSI (Relative Strength Index)
        _listings.Add(Rsi.BufferListing);
        _listings.Add(Rsi.SeriesListing);
        _listings.Add(Rsi.StreamListing);

        // Slope
        _listings.Add(Slope.SeriesListing);
        _listings.Add(Slope.BufferListing);
        _listings.Add(Slope.StreamListing);

        // SMA (Simple Moving Average)
        _listings.Add(Sma.SeriesListing);
        _listings.Add(Sma.StreamListing);
        _listings.Add(Sma.BufferListing);

        // SMA Analysis
        _listings.Add(SmaAnalysis.BufferListing);
        _listings.Add(SmaAnalysis.SeriesListing);
        _listings.Add(SmaAnalysis.StreamListing);

        // SMI (Stochastic Momentum Index)
        _listings.Add(Smi.BufferListing);
        _listings.Add(Smi.SeriesListing);
        _listings.Add(Smi.StreamListing);

        // SMMA (Smoothed Moving Average)
        _listings.Add(Smma.SeriesListing);
        _listings.Add(Smma.StreamListing);
        _listings.Add(Smma.BufferListing);

        // STARC Bands
        _listings.Add(StarcBands.BufferListing);
        _listings.Add(StarcBands.SeriesListing);
        _listings.Add(StarcBands.StreamListing);

        // STC (Schaff Trend Cycle)
        _listings.Add(Stc.BufferListing);
        _listings.Add(Stc.SeriesListing);
        _listings.Add(Stc.StreamListing);

        // Standard deviation
        _listings.Add(StdDev.BufferListing);
        _listings.Add(StdDev.SeriesListing);
        _listings.Add(StdDev.StreamListing);

        // Standard Deviation Channels
        _listings.Add(StdDevChannels.SeriesListing);

        // Stochastic Oscillator
        _listings.Add(Stoch.BufferListing);
        _listings.Add(Stoch.SeriesListing);
        _listings.Add(Stoch.StreamListing);

        // Stochastic RSI
        _listings.Add(StochRsi.BufferListing);
        _listings.Add(StochRsi.SeriesListing);
        _listings.Add(StochRsi.StreamListing);

        // SuperTrend
        _listings.Add(SuperTrend.BufferListing);
        _listings.Add(SuperTrend.SeriesListing);
        _listings.Add(SuperTrend.StreamListing);

        // T3 (Triple Exponential Moving Average)
        _listings.Add(T3.BufferListing);
        _listings.Add(T3.SeriesListing);
        _listings.Add(T3.StreamListing);

        // TEMA (Triple Exponential Moving Average)
        _listings.Add(Tema.BufferListing);
        _listings.Add(Tema.SeriesListing);
        _listings.Add(Tema.StreamListing);

        // TR (True Range)
        _listings.Add(Tr.BufferListing);
        _listings.Add(Tr.SeriesListing);
        _listings.Add(Tr.StreamListing);

        // TRIX
        _listings.Add(Trix.SeriesListing);
        _listings.Add(Trix.BufferListing);
        _listings.Add(Trix.StreamListing);

        // TSI (True Strength Index)
        _listings.Add(Tsi.SeriesListing);
        _listings.Add(Tsi.BufferListing);
        _listings.Add(Tsi.StreamListing);

        // Ulcer Index
        _listings.Add(UlcerIndex.SeriesListing);
        _listings.Add(UlcerIndex.StreamListing);
        _listings.Add(UlcerIndex.BufferListing);

        // Ultimate Oscillator
        _listings.Add(Ultimate.BufferListing);
        _listings.Add(Ultimate.SeriesListing);
        _listings.Add(Ultimate.StreamListing);

        // Volatility Stop
        _listings.Add(VolatilityStop.SeriesListing);
        _listings.Add(VolatilityStop.BufferListing);
        _listings.Add(VolatilityStop.StreamListing);

        // Vortex Indicator
        _listings.Add(Vortex.SeriesListing);
        _listings.Add(Vortex.BufferListing);
        _listings.Add(Vortex.StreamListing);

        // VWAP (Volume Weighted Average Price)
        _listings.Add(Vwap.SeriesListing);
        _listings.Add(Vwap.BufferListing);
        _listings.Add(Vwap.StreamListing);

        // VWMA (Volume Weighted Moving Average)
        _listings.Add(Vwma.SeriesListing);
        _listings.Add(Vwma.StreamListing);
        _listings.Add(Vwma.BufferListing);

        // Williams %R
        _listings.Add(WilliamsR.BufferListing);
        _listings.Add(WilliamsR.SeriesListing);
        _listings.Add(WilliamsR.StreamListing);

        // WMA (Weighted Moving Average)
        _listings.Add(Wma.BufferListing);
        _listings.Add(Wma.SeriesListing);
        _listings.Add(Wma.StreamListing);

        // ZigZag
        _listings.Add(ZigZag.SeriesListing);
    }
}
