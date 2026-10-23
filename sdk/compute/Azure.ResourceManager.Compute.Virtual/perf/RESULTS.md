# Virtual Property Performance Results

## Environment

- BenchmarkDotNet 0.15.8
- .NET SDK 10.0.402, runtime 10.0.13, X64 RyuJIT
- Concurrent Server GC
- Windows under Hyper-V, AMD EPYC 7763
- Normal JIT tiering, inlining, devirtualization, and dynamic PGO enabled

The benchmarks compare the original `Azure.ResourceManager.Compute` assembly with
the `Azure.ResourceManager.Compute.Virtual` copy. Both use the same synthetic
`VirtualMachine.json` response and generated model contexts. No network calls or
mocking framework are involved.

## Direct Property Read

The property-read benchmark returns `VirtualMachineData.ETag`, a getter-only scalar
property declared directly on the resource model. It is not flattened and does not
forward through the internal `Properties` model.

```csharp
[Benchmark(Baseline = true)]
public string ReadETagOriginal() => _original.ETag;

[Benchmark]
public string ReadETagVirtual() => _virtual.ETag;
```

Three process launches, eight warmups, and 25 measured iterations per launch:

| Metric | Original | Virtual | Delta |
| --- | ---: | ---: | ---: |
| Mean | 1.770 ns | 2.763 ns | +0.993 ns |
| Error: 99.9% CI half-width | 0.158 ns | 0.164 ns | N/A |
| Allocated | 0 B | 0 B | 0 B |

The original distribution was multimodal, so the percentage ratio should not be
treated as stable. The absolute difference is the more useful observation.

JIT disassembly from a separate diagnostic run shows the expected code shape:

- Original: 12 bytes; load the VM field, load the ETag backing field, return.
- Virtual: 38 bytes; load the VM field, compare its exact type, load the ETag field
  on the fast path, or jump to the virtual getter for a derived type.

## VirtualMachineData Serialization and Deserialization

The serialization benchmarks use a populated VM response containing image, OS,
storage, two data disks, two NICs, tags, security, diagnostics, zones, and instance
view data. Each timed method calls the actual generated SDK path:

```csharp
ModelReaderWriter.Write(model, options, context);
ModelReaderWriter.Read<VirtualMachineData>(response, options, context);
```

Two independent complete runs were performed. Each run used two process launches,
five warmups, and 15 measured iterations per launch. Positive delta means the
virtual assembly had a greater elapsed time.

| Operation | Format | Run 1 original | Run 1 virtual | Run 1 delta | Run 2 original | Run 2 virtual | Run 2 delta |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Serialize | J | 5.843 us | 6.016 us | +3.0% | 5.629 us | 6.069 us | +7.8% |
| Serialize | W | 3.648 us | 3.900 us | +6.9% | 3.611 us | 3.820 us | +5.8% |
| Deserialize | J | 16.807 us | 16.344 us | -2.8% | 16.570 us | 16.239 us | -2.0% |
| Deserialize | W | 16.345 us | 16.390 us | +0.3% | 16.571 us | 16.332 us | -1.4% |

Run 2 reported matching allocations for both assemblies:

| Operation | Format | Allocated per operation |
| --- | --- | ---: |
| Serialize | J | 4,384 B |
| Serialize | W | 2,192 B |
| Deserialize | J or W | 9,200 B |

The W serialization slowdown reproduced in both runs, with an absolute increase of
approximately 0.21-0.25 us per VM. Deserialization did not show a consistent
regression. Lower deserialization means in some rows should not be interpreted as
an intrinsic benefit of virtual properties.

## Interpretation and Limits

For the direct ETag getter, guarded devirtualization leaves an exact-type comparison
and a derived-type fallback that are absent from the original direct field load.
Serialization repeatedly reads eligible properties across the VM object graph, so
similar guards can accumulate. Deserialization primarily parses into local values
and constructs concrete types, allowing the JIT to eliminate or guard many setter
dispatches differently.

These measurements cover one VM payload on one virtualized machine and runtime.
They do not measure cold start, AOT, HTTP pipeline latency, or Moq proxy overhead.
Very small property-access measurements are sensitive to BenchmarkDotNet overhead
subtraction and code layout. Results should be repeated on representative physical
hardware before setting an SDK-wide performance threshold.

## Commands

From this directory:

```powershell
dotnet run -c Release -f net10.0 -- --validate
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachinePropertyReadBenchmarks*" --launchCount 3 --warmupCount 8 --iterationCount 25 --buildTimeout 600
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachineSerializationBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15 --buildTimeout 600
```

Raw BenchmarkDotNet artifacts are intentionally ignored; this file records the
reviewable result summary checked into the PR.
