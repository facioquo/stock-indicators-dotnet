using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage(
    "Maintainability",
    "CA1515:Consider making public types internal",
    Justification = "Test projects use public class types.")]

[assembly: SuppressMessage(
    "Security",
    "CA5394:Do not use insecure randomness",
    Justification = "Okay for test rig, non-production code.")]
