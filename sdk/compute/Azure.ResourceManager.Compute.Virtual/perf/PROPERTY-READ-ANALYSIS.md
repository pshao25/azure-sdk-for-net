# Compute VM Direct Property Read Analysis

## Workload

`VirtualMachinePropertyReadBenchmarks` compares one direct, non-flattened property:

```csharp
return vm.Zones;
```

`Zones` is a getter-only auto-property declared directly on `VirtualMachineData`.
It does not forward through the internal `Properties` model. The baseline uses the
original Compute SDK and the comparison uses the source-identical virtualized SDK.
Setup deserializes one model per assembly, verifies getter modifiers, and checks the
fixture value outside timing. Returning the collection reference makes the result
observable to BenchmarkDotNet. There is no loop, batching, `Length`, `Count`,
checksum, sink field, serialization, or allocation in the timed methods.

## Timing

Measured on 2026-10-21 with BenchmarkDotNet 0.15.8, .NET SDK 10.0.402,
runtime 10.0.13, X64 RyuJIT, Concurrent Server GC, Windows under Hyper-V,
AMD EPYC 7763. Three launches, eight warmups and 25 measured iterations per launch:

| Metric | Original (non-virtual) | Virtual | Delta |
| --- | ---: | ---: | ---: |
| Mean | 1.063 ns | 1.343 ns | +0.280 ns (+26.3%) |
| Error: 99.9% CI half-width | 0.038 ns | 0.024 ns | N/A |
| Allocated | 0 B | 0 B | 0 B |

The original distribution was multimodal and both methods are near the overhead
subtraction/noise region, so the approximately 26% ratio should not be generalized.
The more useful observation is that this direct getter adds only about **0.28 ns**
in this environment. Timing reports are under `BenchmarkDotNet.Artifacts/property-read-zones`.

## JIT Assembly

A separate ShortRun produced 2.060 vs 2.569 ns and captured the complete methods
under `BenchmarkDotNet.Artifacts/property-read-zones-asm/results`.

The original method is 9 B and fully inlines the getter:

```assembly
mov rax,[benchmark+originalVmOffset]
mov rax,[rax+zonesFieldOffset]
ret
```

The virtual method is 35 B. Guarded devirtualization adds one exact-type check; the
expected type still takes a direct field-load fast path, while a subclass falls back
to its vtable slot:

```assembly
mov rcx,[benchmark+virtualVmOffset]
cmp [rcx],MT_VirtualMachineData
jne virtualFallback
mov rax,[rcx+zonesFieldOffset]
ret
```

## Interpretation

For a one-layer, non-flattened auto-property, the virtual cost is one guarded type
comparison and branch on the common exact-type path. The measured absolute overhead
is sub-nanosecond and much smaller than the previous four-layer flattened `IsEnabled`
experiment (about +2.37 ns), where the virtual getter was not fully inlined and each
nested model layer retained guards.

This comparison is specific to a getter-only reference property and this JIT/runtime.
Writable properties, value types, polymorphic object distributions, or different
call sites can produce different code. Diagnostic assembly was collected separately;
no hardware-counter profiling was performed.
