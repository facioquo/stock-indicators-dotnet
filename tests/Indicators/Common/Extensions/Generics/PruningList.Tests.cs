namespace Utilities;

[TestClass]
public class PruningListTests : TestBase
{
    [TestMethod]
    public void Add_ThenIndex_ReflectsInsertionOrder()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 5; i++)
        {
            sut.Add(i);
        }

        sut.Count.Should().Be(5);
        sut[0].Should().Be(0);
        sut[4].Should().Be(4);
        sut.Should().ContainInOrder(0, 1, 2, 3, 4);
    }

    [TestMethod]
    public void RemoveRange_FromFront_RetainsTailWindow()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 10; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 3);

        sut.Count.Should().Be(7);
        sut[0].Should().Be(3);
        sut.Should().ContainInOrder(3, 4, 5, 6, 7, 8, 9);
    }

    [TestMethod]
    public void RemoveRange_FromMiddle_RemovesInteriorElements()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 10; i++)
        {
            sut.Add(i);
        }

        // prune front first so a head offset is active
        sut.RemoveRange(0, 2);
        sut.RemoveRange(3, 2); // removes live indices 3,4 => values 5,6

        sut.Should().ContainInOrder(2, 3, 4, 7, 8, 9);
    }

    [TestMethod]
    public void RemoveAt_Front_AdvancesWindow()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 4; i++)
        {
            sut.Add(i);
        }

        sut.RemoveAt(0);

        sut.Count.Should().Be(3);
        sut[0].Should().Be(1);
    }

    [TestMethod]
    public void Insert_WithActiveHead_PlacesAtLiveIndex()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 6; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 2); // live: 2,3,4,5
        sut.Insert(1, 99);     // live: 2,99,3,4,5

        sut.Should().ContainInOrder(2, 99, 3, 4, 5);
    }

    [TestMethod]
    public void RemoveAll_WithActiveHead_RemovesMatchingLiveItems()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 10; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 3); // live: 3..9

        int removed = sut.RemoveAll(static x => x % 2 == 0);

        removed.Should().Be(3); // 4, 6, 8
        sut.Should().ContainInOrder(3, 5, 7, 9);
    }

    [TestMethod]
    public void IndexOf_WithActiveHead_ReturnsLiveRelativeIndex()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 6; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 2); // live: 2,3,4,5

        sut.IndexOf(4).Should().Be(2);
        sut.IndexOf(2).Should().Be(0);
        sut.IndexOf(0).Should().Be(-1); // pruned
        sut.Contains(3).Should().BeTrue();
        sut.Contains(1).Should().BeFalse();
    }

    [TestMethod]
    public void Remove_LiveItem_RemovesAndReturnsTrue()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 5; i++)
        {
            sut.Add(i);
        }

        sut.Remove(2).Should().BeTrue();
        sut.Remove(99).Should().BeFalse();
        sut.Should().ContainInOrder(0, 1, 3, 4);
    }

    [TestMethod]
    public void ToArrayAndCopyTo_ReturnLiveWindowOnly()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 8; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 5); // live: 5,6,7

        sut.ToArray().Should().Equal(5, 6, 7);

        int[] buffer = new int[3];
        sut.CopyTo(buffer, 0);
        buffer.Should().Equal(5, 6, 7);
    }

    [TestMethod]
    public void Clear_ResetsCountAndHead()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 5; i++)
        {
            sut.Add(i);
        }

        sut.RemoveRange(0, 2);
        sut.Clear();

        sut.Count.Should().Be(0);

        sut.Add(42);
        sut[0].Should().Be(42);
    }

    [TestMethod]
    public void Indexer_OutOfRange_Throws()
    {
        PruningList<int> sut = [];
        sut.Add(1);
        sut.RemoveRange(0, 1); // empty live window, head active

        Action read = () => _ = sut[0];
        read.Should().Throw<ArgumentOutOfRangeException>();
    }

    [TestMethod]
    public void RemoveRange_BeyondLiveWindow_Throws()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 3; i++)
        {
            sut.Add(i);
        }

        Action act = () => sut.RemoveRange(0, 5);
        act.Should().Throw<ArgumentException>();
    }

    [TestMethod]
    public void Enumerate_WhenModified_Throws()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 5; i++)
        {
            sut.Add(i);
        }

        Action act = () => {
            foreach (int _ in sut)
            {
                sut.Add(100);
            }
        };

        act.Should().Throw<InvalidOperationException>();
    }

    [TestMethod]
    public void AsReadOnly_ReflectsLiveWindowAndIsImmutable()
    {
        PruningList<int> sut = [];

        for (int i = 0; i < 6; i++)
        {
            sut.Add(i);
        }

        IReadOnlyList<int> view = ((IList<int>)sut).AsReadOnly();

        // not castable to mutable list
        (view is List<int>).Should().BeFalse();

        // reflects live window after pruning
        sut.RemoveRange(0, 4);
        view.Should().ContainInOrder(4, 5);
        view.Count.Should().Be(2);
    }

    [TestMethod]
    public void StreamingPrune_AtSteadyState_MatchesSlidingWindowOracle()
    {
        const int cap = 500;
        const int total = 50_000;

        PruningList<int> sut = [];

        for (int i = 0; i < total; i++)
        {
            sut.Add(i);

            if (sut.Count > cap)
            {
                sut.RemoveRange(0, sut.Count - cap);
            }
        }

        // exact-count semantics preserved
        sut.Count.Should().Be(cap);

        // live window is the most recent `cap` values, in order
        IEnumerable<int> expected = Enumerable.Range(total - cap, cap);
        sut.Should().ContainInOrder(expected);
        sut[0].Should().Be(total - cap);
        sut[cap - 1].Should().Be(total - 1);
    }
}
