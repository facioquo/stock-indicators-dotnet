using BenchmarkDotNet.Characteristics;
using BenchmarkDotNet.Running;
using BenchmarkDotNet.Toolchains;
using BenchmarkDotNet.Toolchains.CsProj;
using BenchmarkDotNet.Toolchains.DotNetCli;
using BenchmarkDotNet.Validators;

namespace Performance;

/// <summary>
/// Builds and runs benchmarks as <c>net11.0</c> child processes. BenchmarkDotNet 0.15.8
/// has no .NET 11 runtime moniker, so its SDK validator throws on a .NET 11 host; this
/// toolchain reuses the standard project toolchain and skips all of its validation.
/// Remove it once a stable BenchmarkDotNet release recognizes .NET 11.
/// </summary>
internal sealed class Net11Toolchain : IToolchain
{
    private readonly IToolchain _inner = CsProjCoreToolchain.From(
        new NetCoreAppSettings("net11.0", runtimeFrameworkVersion: null, name: ".NET 11.0"));

    internal static Net11Toolchain Instance { get; } = new();

    public string Name => _inner.Name;

    public IGenerator Generator => _inner.Generator;

    public IBuilder Builder => _inner.Builder;

    public IExecutor Executor => _inner.Executor;

    public bool IsInProcess => _inner.IsInProcess;

    public IEnumerable<ValidationError> Validate(BenchmarkCase benchmarkCase, IResolver resolver) => [];
}
