using System.Runtime.CompilerServices;

[assembly: CLSCompliant(true)]
[assembly: InternalsVisibleTo("Tests.Integration")] // these use test data
[assembly: InternalsVisibleTo("Tests.PublicApi")]
[assembly: InternalsVisibleTo("Tests.Performance")]
[assembly: InternalsVisibleTo("Test.DataGenerator")]
[assembly: InternalsVisibleTo("Test.SseServer")] // SSE server uses test data infrastructure

[assembly: Parallelize(Scope = ExecutionScope.MethodLevel, Workers = 0)]
