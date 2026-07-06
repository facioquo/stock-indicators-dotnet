namespace Test.Tools;

/// <summary>
/// Custom assertion helpers for test validation.
/// </summary>
public static class TestAssert
{
    /// <summary>
    /// Asserts that all computed (non-null, non-NaN) values remain within inclusive bounds.
    /// </summary>
    /// <typeparam name="T">Result type containing the bounded value.</typeparam>
    /// <param name="results">Result collection to evaluate.</param>
    /// <param name="selector">Selector that returns the bounded value to check.</param>
    /// <param name="minInclusive">Minimum inclusive bound.</param>
    /// <param name="maxInclusive">Maximum inclusive bound.</param>
    [StackTraceHidden]
    public static void IsBetween<T>(
        this IEnumerable<T> results,
        Func<T, double?> selector,
        double minInclusive,
        double maxInclusive)
    {
        ArgumentNullException.ThrowIfNull(results);
        ArgumentNullException.ThrowIfNull(selector);

        foreach (T result in results)
        {
            double? value = selector(result);

            if (!value.HasValue || double.IsNaN(value.Value))
            {
                continue;
            }

            value.Value.Should().BeGreaterThanOrEqualTo(minInclusive);
            value.Value.Should().BeLessThanOrEqualTo(maxInclusive);
        }
    }

    /// <summary>
    /// Asserts that two <see cref="IEnumerable{T}" /> series are exactly the same.
    /// </summary>
    /// <typeparam name="T">List elements must be <see cref="ISeries"/> types.</typeparam>
    /// <param name="actuals">Actual collection subject under test (sut).</param>
    /// <param name="expected">Expected collection to match exactly.</param>
    /// <param name="because">
    /// Optional formatted phrase, supported by <see cref="string.Format(string,object[])" />,
    /// explaining why the assertion is needed. If the phrase does not start with the word
    /// <i>because</i>, it is prepended automatically.
    /// </param>
    /// <param name="becauseArgs">
    /// Zero or more objects to format using the placeholders in <paramref name="because" />.
    /// </param>
    /// <remarks>
    /// Collections are compared using strict ordering and by comparing all members of <typeparamref name="T"/>.
    /// </remarks>
    [StackTraceHidden]
    public static void IsExactly<T>(
        this IEnumerable<T> actuals,
        IEnumerable<T> expected,
        string because = "",
        params object[] becauseArgs
    ) where T : ISeries
        => actuals.Should().BeEquivalentTo(
            expectation: expected,
            config: static options => options
                .WithStrictOrdering()     // require same sequence order
                .ComparingByMembers<T>(), // enforces value-based structural equivalency
            because: because,
            becauseArgs: becauseArgs);
}
