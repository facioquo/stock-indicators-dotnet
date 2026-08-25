using System.Diagnostics.CodeAnalysis;

// Code Analysis suppressions specifically covering obsolete shims and other deprecations.
// These are not intended to be permanent, but they are necessary to omit false positives.

[assembly: SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Temporary, during deprecation period.",
    Scope = "member",
    Target = "~P:FacioQuo.Stock.Indicators.ISeries.Date")]
