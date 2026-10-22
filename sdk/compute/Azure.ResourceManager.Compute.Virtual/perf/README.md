# Compute Serialization Comparison

See [RESULTS.md](RESULTS.md) for two completed local runs and their limitations.

Benchmarks the actual `VirtualMachineData` from the current sibling Compute SDK
against its reproducibly virtualized clone. Both reference identical ARM/Core
dependencies and retain all original serialization code. Assembly aliases distinguish
the two SDKs without changing namespaces. See [the clone contract](../README.md).

`VirtualMachine.json` is a synthetic, response-shaped Linux VM fixture inspired by
Compute's `tests/Helpers/ResourceDataHelper.cs`, extended with two data disks, two
NICs, security/diagnostics profiles, tags, zones and instance view. It is not a
captured live response and contains no passwords, tokens, or keys. No Azure access
is required. This tests the real SDK, not a made-up model or a full HTTP pipeline.

## Run

Use the repository-required SDK (currently .NET 10.0.400). From this directory:

```powershell
dotnet run -c Release -f net10.0 -- --validate
dotnet run -c Release -f net10.0 -- --filter "*" --job short
dotnet run -c Release -f net10.0 -- --filter "*" --launchCount 2 --warmupCount 5 --iterationCount 15
```

Before comparing an updated Compute SDK, regenerate/verify the clone using the
commands in its README. The transformation manifest records exact source/output
hashes; do not hand-edit the cloned SDK. The original API checks are not disabled.

## Methodology

### Property Read Comparison

`VirtualMachinePropertyReadBenchmarks` reads only `VirtualMachineData.Zones`, a
getter-only auto-property declared directly on the resource model. It is not
flattened and does not forward through the internal properties wrapper. The two
benchmark methods directly return `IList<string>`; there is no loop, batching,
`Length`, `Count`, checksum, sink field, serialization, or allocation in timed code.
Setup verifies modifiers and the fixture value outside timing.
Measured results and annotated JIT evidence are in [PROPERTY-READ-ANALYSIS.md](PROPERTY-READ-ANALYSIS.md).

```powershell
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachinePropertyReadBenchmarks*" --launchCount 3 --warmupCount 8 --iterationCount 25 --buildTimeout 600 --artifacts "BenchmarkDotNet.Artifacts/property-read-zones"
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachinePropertyReadBenchmarks*" --job short --disasm --disasmDepth 2 --buildTimeout 600 --artifacts "BenchmarkDotNet.Artifacts/property-read-zones-asm"
```

Timing and assembly are collected separately. Normal JIT/PGO optimizations are
allowed; no artificial `NoInlining` or overriding subclasses are introduced.

### Serialization Comparison

- Four methods, each in `J` and `W`, give eight scenarios. Each virtual method has
  an original-SDK baseline in the same category and format.
- Serialization calls `ModelReaderWriter.Write` using each SDK's generated context.
- Deserialization calls `ModelReaderWriter.Read<VirtualMachineData>` using that
  SDK's context and the same response bytes, including read-only fields in W mode.
- Setup reads the payload, warms both paths, verifies resource inheritance and
  outer/internal virtual flags, checks parsed nested values, compares serialized
  bytes, checks read-only wire visibility, and validates round trips.
- No I/O, reflection, validation, source transformation, mocks, or network calls
  occur in timed methods. Models, payloads, options and contexts are prepared/warmed.
- Returned objects/BinaryData prevent dead-code elimination. Normal JIT inlining,
  devirtualization and PGO remain enabled; this is warmed throughput, not cold start.

The clone virtualizes all eligible class properties, including internal wrappers
and helper collections, not only public resource getters. Inherited properties from
the shared ARM dependency are unchanged. Thus this experiment measures that complete
policy, not just changing one getter on `VirtualMachineData`.

Reports and logs are saved locally under `BenchmarkDotNet.Artifacts` and ignored by
Git. Compare confidence intervals, absolute differences and allocations, not just
ratios; ShortRun results are preliminary. Repeat on idle representative hardware
before concluding that a small difference is stable.
