namespace BufferLists;

[TestClass]
public class ParabolicSarBufferListTests : BufferListTestBase, ITestBarBufferList
{
    private const double accelerationStep = 0.02;
    private const double maxAccelerationFactor = 0.2;

    private static readonly IReadOnlyList<ParabolicSarResult> series
        = Bars.ToParabolicSar(accelerationStep, maxAccelerationFactor);

    [TestMethod]
    public void AddBar_IncrementsResults()
    {
        ParabolicSarList sut = new(accelerationStep, maxAccelerationFactor);

        foreach (Bar q in Bars) { sut.Add(q); }

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void AddBarsBatch_IncrementsResults()
    {
        ParabolicSarList sut = Bars.ToParabolicSarList(accelerationStep, maxAccelerationFactor);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void BarsCtor_OnInstantiation_IncrementsResults()
    {
        ParabolicSarList sut = new(
            accelerationStep,
            maxAccelerationFactor,
            accelerationStep,
            Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(series);
    }

    [TestMethod]
    public void Extended_WithInitialStep_ReturnsExpectedResult()
    {
        const double accelerationStepExt = 0.02;
        const double maxAccelerationFactorExt = 0.2;
        const double initialStep = 0.01;

        IReadOnlyList<ParabolicSarResult> seriesExt =
            Bars.ToParabolicSar(accelerationStepExt, maxAccelerationFactorExt, initialStep);

        ParabolicSarList sut = new(
            accelerationStepExt,
            maxAccelerationFactorExt,
            initialStep,
            Bars);

        sut.Should().HaveCount(Bars.Count);
        sut.IsExactly(seriesExt);
    }

    [TestMethod]
    public override void PruneList_OverMaxListSize_AutoAdjustsListAndBuffers()
    {
        const int maxListSize = 120;

        ParabolicSarList sut = new(accelerationStep, maxAccelerationFactor) {
            MaxListSize = maxListSize
        };

        sut.Add(Bars);

        IReadOnlyList<ParabolicSarResult> expected
            = series.Skip(series.Count - maxListSize).ToList();

        sut.Should().HaveCount(maxListSize);
        sut.IsExactly(expected);
    }

    [TestMethod]
    public override void Clear_WithState_ResetsState()
    {
        List<Bar> subset = Bars.Take(80).ToList();
        IReadOnlyList<ParabolicSarResult> expected = subset.ToParabolicSar(accelerationStep, maxAccelerationFactor);

        ParabolicSarList sut = new(
            accelerationStep,
            maxAccelerationFactor,
            accelerationStep,
            subset);

        sut.Should().HaveCount(subset.Count);
        sut.IsExactly(expected);

        sut.Clear();

        sut.Should().BeEmpty();

        sut.Add(subset);

        sut.Should().HaveCount(expected.Count);
        sut.IsExactly(expected);
    }
}
