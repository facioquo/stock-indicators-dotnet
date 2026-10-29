namespace Performance;

// BUFFER-STYLE INDICATORS

[ShortRunJob, WarmupCount(5), IterationCount(5)]
public class BufferIndicators
{
    private static readonly IReadOnlyList<Bar> q = Data.GetDefault();
    private const int n = 14;

    /* Parameter arguments should match the Perf.Series.cs equivalents */

    [Benchmark] public AdlList AdlList() => q.ToAdlList();
    [Benchmark] public AdxList AdxList() => q.ToAdxList(n);
    [Benchmark] public AlligatorList AlligatorList() => q.ToAlligatorList();
    [Benchmark] public AlmaList AlmaList() => q.ToAlmaList(9, 0.85, 6);
    [Benchmark] public AroonList AroonList() => q.ToAroonList();
    [Benchmark] public AtrList AtrList() => q.ToAtrList(n);
    [Benchmark] public AtrStopList AtrStopList() => q.ToAtrStopList();
    [Benchmark] public AwesomeList AwesomeList() => q.ToAwesomeList();
    [Benchmark] public BollingerBandsList BollingerBandsList() => q.ToBollingerBandsList(20, 2);
    [Benchmark] public BopList BopList() => q.ToBopList(n);
    [Benchmark] public CciList CciList() => q.ToCciList(n);
    [Benchmark] public ChaikinOscList ChaikinOscList() => q.ToChaikinOscList();
    [Benchmark] public ChandelierList ChandelierList() => q.ToChandelierList();
    [Benchmark] public ChopList ChopList() => q.ToChopList(n);
    [Benchmark] public CmfList CmfList() => q.ToCmfList(n);
    [Benchmark] public CmoList CmoList() => q.ToCmoList(n);
    [Benchmark] public ConnorsRsiList ConnorsRsiList() => q.ToConnorsRsiList(3, 2, 100);
    [Benchmark] public DemaList DemaList() => q.ToDemaList(n);
    [Benchmark] public DojiList DojiList() => q.ToDojiList();
    [Benchmark] public DonchianList DonchianList() => q.ToDonchianList(20);
    [Benchmark] public DpoList DpoList() => q.ToDpoList(n);
    [Benchmark] public DynamicList DynamicList() => q.ToDynamicList(n);
    [Benchmark] public ElderRayList ElderRayList() => q.ToElderRayList(13);
    [Benchmark] public EmaList EmaList() => q.ToEmaList(20);
    [Benchmark] public EpmaList EpmaList() => q.ToEpmaList(n);
    [Benchmark] public FcbList FcbList() => q.ToFcbList(2);
    [Benchmark] public FisherTransformList FisherTransformList() => q.ToFisherTransformList(10);
    [Benchmark] public ForceIndexList ForceIndexList() => q.ToForceIndexList(2);
    [Benchmark] public FractalList FractalList() => q.ToFractalList();
    [Benchmark] public GatorList GatorList() => q.ToGatorList();
    [Benchmark] public HeikinAshiList HeikinAshiList() => q.ToHeikinAshiList();
    [Benchmark] public HmaList HmaList() => q.ToHmaList(n);
    [Benchmark] public HtTrendlineList HtTrendlineList() => q.ToHtTrendlineList();
    [Benchmark] public HurstList HurstList() => q.ToHurstList(100);
    [Benchmark] public IchimokuList IchimokuList() => q.ToIchimokuList();
    [Benchmark] public KamaList KamaList() => q.ToKamaList(10, 2, 30);
    [Benchmark] public KeltnerList KeltnerList() => q.ToKeltnerList(20, 2, 10);
    [Benchmark] public KvoList KvoList() => q.ToKvoList(34, 55, 13);
    [Benchmark] public MacdList MacdList() => q.ToMacdList(12, 26, 9);
    [Benchmark] public MaEnvelopesList MaEnvelopesList() => q.ToMaEnvelopesList(20, 2.5, MaType.SMA);
    [Benchmark] public MamaList MamaList() => q.ToMamaList(0.5, 0.05);
    [Benchmark] public MarubozuList MarubozuList() => q.ToMarubozuList(95);
    [Benchmark] public MfiList MfiList() => q.ToMfiList(14);
    [Benchmark] public ObvList ObvList() => q.ToObvList();
    [Benchmark] public ParabolicSarList ParabolicSarList() => q.ToParabolicSarList();
    [Benchmark] public PivotPointsList PivotPointsList() => q.ToPivotPointsList(BarInterval.Month, PivotPointType.Standard);
    [Benchmark] public PivotsList PivotsList() => q.ToPivotsList(2, 2, 20);
    [Benchmark] public PmoList PmoList() => q.ToPmoList(35, 20, 10);
    [Benchmark] public BarPartList BarPartList() => q.ToBarPartList(CandlePart.OHL3);
    [Benchmark] public PvoList PvoList() => q.ToPvoList();
    [Benchmark] public RenkoList RenkoList() => q.ToRenkoList(2.5m);
    [Benchmark] public RocList RocList() => q.ToRocList(20);
    [Benchmark] public RollingPivotsList RollingPivotsList() => q.ToRollingPivotsList(20, 0, PivotPointType.Standard);
    [Benchmark] public RocWbList RocWbList() => q.ToRocWbList(20, 5, 5);
    [Benchmark] public RsiList RsiList() => q.ToRsiList(n);
    [Benchmark] public SlopeList SlopeList() => q.ToSlopeList(n);
    [Benchmark] public SmaList SmaList() => q.ToSmaList(n);
    [Benchmark] public SmaAnalysisList SmaAnalysisList() => q.ToSmaAnalysisList(n);
    [Benchmark] public SmiList SmiList() => q.ToSmiList(13, 25, 2, 3);
    [Benchmark] public SmmaList SmmaList() => q.ToSmmaList(n);
    [Benchmark] public StarcBandsList StarcBandsList() => q.ToStarcBandsList(5, 2, 10);
    [Benchmark] public StcList StcList() => q.ToStcList(10, 23, 50);
    [Benchmark] public StdDevList StdDevList() => q.ToStdDevList(n);
    [Benchmark] public StochList StochList() => q.ToStochList(14, 3, 3);
    [Benchmark] public StochRsiList StochRsiList() => q.ToStochRsiList(14, 14, 3, 1);
    [Benchmark] public SuperTrendList SuperTrendList() => q.ToSuperTrendList(10, 3);
    [Benchmark] public T3List T3List() => q.ToT3List(5, 0.7);
    [Benchmark] public TemaList TemaList() => q.ToTemaList(n);
    [Benchmark] public TrList TrList() => q.ToTrList();
    [Benchmark] public TrixList TrixList() => q.ToTrixList(n);
    [Benchmark] public TsiList TsiList() => q.ToTsiList(25, 13, 7);
    [Benchmark] public TtmSqueezeList TtmSqueezeList() => q.ToTtmSqueezeList(20, 2, 1.5);
    [Benchmark] public UlcerIndexList UlcerIndexList() => q.ToUlcerIndexList(n);
    [Benchmark] public UltimateList UltimateList() => q.ToUltimateList(7, 14, 28);
    [Benchmark] public VolatilityStopList VolatilityStopList() => q.ToVolatilityStopList(7, 3);
    [Benchmark] public VortexList VortexList() => q.ToVortexList(n);
    [Benchmark] public VwapList VwapList() => q.ToVwapList();
    [Benchmark] public VwmaList VwmaList() => q.ToVwmaList(n);
    [Benchmark] public WilliamsRList WilliamsRList() => q.ToWilliamsRList();
    [Benchmark] public WmaList WmaList() => q.ToWmaList(n);
}
