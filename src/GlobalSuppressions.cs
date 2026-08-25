using System.Diagnostics.CodeAnalysis;

// Code Analysis suppressions specifically covering intended or accepted overrides.
// These must remain specific and targeted and never be used to hide genuine issues.
// Suppressions for the obsolete shims are in Obsolete.*.Suppressions.cs, not here.

[assembly: SuppressMessage(
    "Naming",
    "CA1710:Identifiers should have correct suffix",
    Justification = "BufferList is the established naming convention for this library",
    Scope = "type",
    Target = "~T:FacioQuo.Stock.Indicators.BufferList`1")]

[assembly: SuppressMessage("Naming",
    "CA1720:Identifier contains type name"
    , Justification = "Not really an issue.",
    Scope = "type",
    Target = "~T:FacioQuo.Stock.Indicators.Direction")]

[assembly: SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification = "Not really an issue.",
    Scope = "member",
    Target = "~F:FacioQuo.Stock.Indicators.Direction.Long")]

[assembly: SuppressMessage(
    "Naming",
    "CA1720:Identifier contains type name",
    Justification = "Not really an issue.",
    Scope = "member",
    Target = "~F:FacioQuo.Stock.Indicators.Direction.Short")]
