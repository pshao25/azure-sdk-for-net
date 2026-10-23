# Init-only Property Performance Results

## Environment

- BenchmarkDotNet 0.15.8
- .NET SDK 10.0.402, runtime 10.0.13, X64 RyuJIT
- Concurrent Server GC
- Windows under Hyper-V, AMD EPYC 7763

The benchmarks compare the original `Azure.ResourceManager.Compute` assembly with
the `Azure.ResourceManager.Compute.Init` copy. The Init copy adds public init-only
accessors to eight response properties on `VirtualMachineData`; it does not change
the generated serialization code. Both assemblies use the same synthetic
`VirtualMachine.json` response and generated model contexts.

## Object Initialization

Two process launches, five warmups, and 15 measured iterations per launch:

| Method | Mean | Error: 99.9% CI half-width | Allocated | Ratio |
| --- | ---: | ---: | ---: | ---: |
| Construct original VM | 49.10 ns | 2.781 ns | 240 B | 1.01 |
| Construct Init VM | 47.05 ns | 1.838 ns | 240 B | 0.96 |
| Construct Init VM and set output properties | 114.21 ns | 4.706 ns | 592 B | 2.34 |

Adding init-only accessors does not change the cost or allocation of the existing
constructor path. Setting flattened output properties through an object initializer
creates the internal `VirtualMachineProperties` model on demand. That explicit work
accounts for the additional 67 ns and 352 B in the third row; existing code that
does not initialize response properties does not pay it.

## Serialization and Deserialization

The serialization benchmarks use a populated VM response containing image, OS,
storage, two data disks, two NICs, tags, security, diagnostics, zones, and instance
view data. Each timed method calls the actual generated SDK path:

```csharp
ModelReaderWriter.Write(model, options, context);
ModelReaderWriter.Read<VirtualMachineData>(response, options, context);
```

Two process launches, five warmups, and 15 measured iterations per launch:

| Operation | Format | Original | Init | Ratio | Allocation for both |
| --- | --- | ---: | ---: | ---: | ---: |
| Deserialize | J | 23.248 us | 25.577 us | 1.10 | 9.02 KB |
| Deserialize | W | 24.324 us | 25.211 us | 1.04 | 9.02 KB |
| Serialize | J | 8.629 us | 8.917 us | 1.04 | 4.30 KB |
| Serialize | W | 5.724 us | 5.288 us | 0.93 | 2.14 KB |

Allocations are identical between assemblies for every operation. The timing ratios
range from 0.93 to 1.10 on this virtualized host, with overlapping or broad
confidence intervals. The generated serialization paths are unchanged, so these
single-run timing differences should not be interpreted as an intrinsic cost or
benefit of init-only accessors.

Before measurement, benchmark setup verifies that both assemblies deserialize the
same values and produce byte-identical output. It also verifies that response-only
properties remain present in `J` output and excluded from `W` output, and that both
formats round-trip.

## Interpretation and Limits

The results show no added allocation or code path for existing construction,
serialization, or deserialization. The additional object-initializer cost applies
only when a caller opts into setting output properties, especially flattened
properties that require creation of their internal container.

These measurements cover one VM payload on one virtualized machine and runtime.
They do not measure cold start, AOT, HTTP pipeline latency, or every generated model
shape. Timing comparisons should be repeated on representative physical hardware
before setting an SDK-wide performance threshold.

## Commands

From this directory:

```powershell
dotnet run -c Release -f net10.0 -- --validate
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachineInitializationBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15 --buildTimeout 600 --artifacts "BenchmarkDotNet.Artifacts/initialization"
dotnet run -c Release -f net10.0 --no-build -- --filter "*VirtualMachineSerializationBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15 --buildTimeout 600 --artifacts "BenchmarkDotNet.Artifacts/serialization"
```

Raw BenchmarkDotNet artifacts are intentionally ignored; this file records the
reviewable result summary checked into the PR.
