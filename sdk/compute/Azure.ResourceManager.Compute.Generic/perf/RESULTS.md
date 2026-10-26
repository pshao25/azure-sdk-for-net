# Generic Model Factory Performance Results

## Prototype Semantics

This prototype implements the anonymous-object form of option 3:

```csharp
VirtualMachineData vm = ModelFactory.Create<VirtualMachineData>(new
{
    Name = "vm1",
    Location = AzureLocation.WestUS2,
    ProvisioningState = "Succeeded"
});
```

Property names and values are validated at runtime. The factory can populate
getter-only and flattened generated properties without requiring an accessible
model constructor. Anonymous nested objects create nested model instances.

The optional `current` argument is a template, not an update target:

```csharp
VirtualMachineData failed = ModelFactory.Create(
    new { ProvisioningState = "Failed" },
    current: baseline);
```

The returned root is a new shallow copy and the baseline remains unchanged.
Unchanged nested models and collections are shared. Flattened property updates use
copy-on-write for the internal `Properties` container so they do not mutate the
baseline indirectly.

## Validation

The validation executable covers:

- getter-only, writable, and flattened properties
- anonymous nested model construction
- collection assignment
- `current` template immutability and shallow sharing
- null overrides
- runtime errors for unknown properties and incompatible values
- generated `ModelReaderWriter` serialization and round-trip behavior

## Performance

- BenchmarkDotNet 0.15.8
- .NET SDK 10.0.402, runtime 10.0.13, X64 RyuJIT
- Concurrent Server GC
- Windows under Hyper-V, AMD EPYC 7763
- Two process launches, five warmups, and 15 measured iterations per launch

| Method | Mean | Error: 99.9% CI half-width | Allocated | Ratio |
| --- | ---: | ---: | ---: | ---: |
| Public VM constructor | 50.85 ns | 4.002 ns | 240 B | 1.01 |
| Generic creation with five properties | 5.700 us | 0.160 us | 2,681 B | 113.43 |
| Generic creation from `current` with one override | 726.05 ns | 16.579 ns | 736 B | 14.45 |

The public constructor row initializes only the normal constructor state; it does
not set the five values supplied by the generic row and is therefore a lower-bound
reference, not an equivalent-work comparison. Generic creation pays for runtime
property discovery, validation, model invariant initialization, and reflective
assignment. Starting from `current` avoids most full-construction work and is about
7.9 times faster than generic creation from scratch in this test.

This intentionally direct implementation does not cache reflection metadata. A
production implementation could reduce repeated lookup and invocation costs, but
would add cache design, memory, trimming, and AOT considerations not evaluated by
this API prototype.

## Limits

The prototype is reflection-based and creates incomplete models deliberately for
testing scenarios. It does not recursively clone unchanged object graphs, perform
service validation, support arbitrary conversions, or provide compile-time property
validation. Measurements cover one model shape on one virtualized host and do not
represent HTTP pipeline costs.

## Commands

From this directory:

```powershell
dotnet run -c Release -f net10.0 -- --validate
dotnet run -c Release -f net10.0 --no-build -- --filter "*GenericModelFactoryBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15 --buildTimeout 600 --artifacts "BenchmarkDotNet.Artifacts/generic"
```

Raw BenchmarkDotNet artifacts are intentionally ignored; this file records the
reviewable result summary checked into the PR.
