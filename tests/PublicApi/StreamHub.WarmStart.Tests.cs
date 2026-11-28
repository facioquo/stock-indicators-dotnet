using System.Text.Json;

namespace Behavioral;

[TestClass]
public class WarmStartTests : TestBase
{
    [TestMethod]
    public void PersistedSnapshots_ResumeBitExact()
    {
        IReadOnlyList<Bar> bars = Bars.Take(200).ToList();

        BarHub source = new();
        EmaHub sourceEma = source.ToEmaHub(20);
        RsiHub sourceRsi = source.ToRsiHub(14);
        source.Add(bars.Take(120).ToList());

        string barJson = JsonSerializer.Serialize(source.CaptureSnapshot());
        string emaJson = JsonSerializer.Serialize(sourceEma.CaptureSnapshot());
        string rsiJson = JsonSerializer.Serialize(sourceRsi.CaptureSnapshot());

        // bars are stored as IBar, so read them back as a concrete bar type
        HubSnapshot<Bar> barSnapshot = JsonSerializer.Deserialize<HubSnapshot<Bar>>(barJson);

        BarHub barHub = new();
        EmaHub ema = barHub.ToEmaHub(20);
        RsiHub rsi = barHub.ToRsiHub(14);

        // restore from the root hub outward
        barHub.RestoreSnapshot(new HubSnapshot<IBar>(
            barSnapshot.FormatVersion,
            barSnapshot.HubName,
            barSnapshot.PrunedThrough,
            barSnapshot.Results,
            barSnapshot.State));
        ema.RestoreSnapshot(JsonSerializer.Deserialize<HubSnapshot<EmaResult>>(emaJson));
        rsi.RestoreSnapshot(JsonSerializer.Deserialize<HubSnapshot<RsiResult>>(rsiJson));

        barHub.Add(bars.Skip(120).ToList());

        ema.Results.Should().Equal(bars.ToEma(20));
        rsi.Results.Should().Equal(bars.ToRsi(14));
    }

    [TestMethod]
    public void UnsupportedHub_Throws()
    {
        BarHub barHub = new();
        SmaHub sma = barHub.ToSmaHub(20);
        barHub.Add(Bars.Take(50).ToList());

        Action capture = () => sma.CaptureSnapshot();

        capture.Should().ThrowExactly<NotSupportedException>();
    }
}
