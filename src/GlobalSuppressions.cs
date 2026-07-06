using System.Diagnostics.CodeAnalysis;

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

[assembly: SuppressMessage(
    "Naming",
    "CA1716:Identifiers should not match keywords",
    Justification = "Temporary, during deprecation period.",
    Scope = "member",
    Target = "~P:FacioQuo.Stock.Indicators.ISeries.Date")]

[assembly: SuppressMessage(
    "Naming",
    "CA1724:Type names should not match namespaces",
    Justification = "Chosen deliberately (issue #2138 Option A) to unify the McGinley Dynamic type family under the `Dynamic` name; the `System.Dynamic` namespace is not otherwise referenced by this library.",
    Scope = "type",
    Target = "~T:FacioQuo.Stock.Indicators.Dynamic")]
