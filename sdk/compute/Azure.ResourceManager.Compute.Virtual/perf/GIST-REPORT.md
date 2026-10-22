# Virtual Model Properties: Customer Experience and Performance

## Summary

Adding `virtual` to SDK model properties enables customers to configure getter-only
properties with Moq while preserving the SDK's read-only API contract. It does not
add setters or make response-only fields writable in service requests.

We evaluated the performance cost using the actual Azure Resource Manager Compute
SDK and a source-identical virtualized counterpart. Property-intensive reads became
slower. VM serialization also had higher means in both repeated runs; deserialization
did not show a consistent regression. These are workload-specific results, not an
SDK-wide slowdown estimate.

## Customer Experience

Use the same `VirtualMachineData` resource model as the benchmarks below. In the
experimental virtualized Compute SDK, its read-only `ProvisioningState` getter is
overridable. The model still inherits `TrackedResourceData` and has a public
`VirtualMachineData(AzureLocation location)` constructor.

The relevant generated property forwards to the internal properties model:

```csharp
public virtual string ProvisioningState
{
    get => Properties is null ? default : Properties.ProvisioningState;
}
```

A customer can configure the read-only state without adding a public setter.
The following examples reference the experimental `Azure.ResourceManager.Compute.Virtual`
assembly, whose namespace remains `Azure.ResourceManager.Compute`. They do not work
against the original non-virtual getter in the baseline SDK.

```csharp
using Azure.Core;
using Azure.ResourceManager.Compute;
using Moq;
using NUnit.Framework;

var mock = new Mock<VirtualMachineData>(new AzureLocation("westus2"))
{
    CallBase = true
};
mock.SetupGet(model => model.ProvisioningState).Returns("Succeeded");

VirtualMachineData input = mock.Object;

Assert.That(input.ProvisioningState, Is.EqualTo("Succeeded"));
Assert.That(input.Location, Is.EqualTo(new AzureLocation("westus2")));

input.Tags["environment"] = "test";
Assert.That(input.Tags["environment"], Is.EqualTo("test"));
```

The value can also change during a test:

```csharp
string state = "Creating";
var mock = new Mock<VirtualMachineData>(new AzureLocation("westus2"))
{
    CallBase = true
};
mock.SetupGet(model => model.ProvisioningState).Returns(() => state);

VirtualMachineData input = mock.Object;
Assert.That(input.ProvisioningState, Is.EqualTo("Creating"));

state = "Succeeded";
Assert.That(input.ProvisioningState, Is.EqualTo("Succeeded"));
```

- `SetupGet` controls what the getter returns; `input.ProvisioningState = "Succeeded"` still
  does not compile. This does not mutate an existing service-returned instance.
- `CallBase = true` retains the original implementations of unconfigured virtual
  members, including ordinary property storage and serialization methods.
- `new AzureLocation("westus2")` is passed to the model's accessible constructor.
  Pass an `AzureLocation`, not a plain string, because Moq's constructor-argument
  binding does not apply C# user-defined implicit conversions. Making a
  property virtual does not remove constructor requirements or make internal-only
  constructors accessible to an external mock.
- `ProvisioningState` is flattened: this setup controls the outer getter, not the
  internal `Properties.ProvisioningState` value used by generated serialization.
  Do not expect the mocked state to appear in persisted JSON; response-only state
  is also excluded from wire requests. This example configures what customer code reads.
- Model factories remain an alternative when only fixed read-only values are needed.

The performance tests below use ordinary `VirtualMachineData` objects, **not Moq**,
so proxy overhead is not measured. Their serialization validation is separate from
the getter-configuration experience shown here.

## Real SDK Comparison

Baseline: the current `Azure.ResourceManager.Compute` 1.17.0-beta.1 source snapshot.
Comparison: a sibling `Azure.ResourceManager.Compute.Virtual` assembly, retaining
the same C# namespaces and runtime dependencies. Aliased references let one benchmark
project use both assemblies.

A reproducible Roslyn-based transformation copies the existing source instead of
regenerating it with a potentially different TypeSpec version:

- 1,778 files copied; 2,954 eligible properties virtualized across 672 files.
- All 594 `*.Serialization.cs` files remain byte-identical.
- Includes public/internal model properties, flattened wrappers, and eligible helper
  properties/indexers. Therefore this measures the full experimental policy, not
  only public resource getters.
- Static/private properties, value types, sealed types, explicit-interface members
  and existing virtual/override/abstract members are excluded as appropriate.
- Inherited ARM members and externally linked shared sources are unchanged.
- Only the runtime source tree is copied, not CI, recordings or the original tests.
  Assembly/package identity and nonshipping build metadata necessarily differ.

Both SDKs built in Release/net10.0. Transformation self-tests and source hash
verification passed. The baseline was not edited and retained its API checks.

## Benchmark Setup

- BenchmarkDotNet 0.15.8; SDK 10.0.400; runtime .NET 10.0.11, X64 RyuJIT.
- Windows 11 under Hyper-V; AMD EPYC 7763, 16 logical / 8 physical cores exposed.
- Concurrent Server GC; normal JIT inlining, tiering and dynamic PGO enabled.
- Two launches, five warmups and 15 measurement iterations per launch.
- Serialization/deserialization ran twice independently, eight scenarios per run.
- Setup, JSON loading, correctness checks and reflection are outside timed methods.

The attached `VirtualMachine.json` is a synthetic Linux VM response-shaped fixture,
not a live recording. It exercises the actual `VirtualMachineData` resource model,
including OS/image/storage profiles, two data disks, two NICs, security, diagnostics,
tags, zones and instance view. It contains no credentials. No HTTP calls are made.

## Property Read Test

`VirtualMachinePropertyReadBenchmarks.cs` reads one direct, non-flattened property
and returns it:

```csharp
public IList<string> ReadZonesVirtual() => _virtual.Zones;
```

The original benchmark is identical except for the non-virtual SDK type. Setup
deserializes one model per assembly and verifies the getter modifiers and fixture
value outside timing. There is no loop, batching, `Length`, `Count`, checksum,
sink field, serialization, or allocation in the timed methods.

`Zones` is a getter-only auto-property declared directly on `VirtualMachineData`.
It does not use the internal `Properties` wrapper.

| Metric | Original | Virtual | Delta (Virtual - Original) |
| --- | ---: | ---: | ---: |
| Mean | 1.063 ns | 1.343 ns | +0.280 ns (+26.3%) |
| Error: 99.9% CI half-width | 0.038 ns | 0.024 ns | N/A |
| Allocated | 0 B | 0 B | 0 B |

Both methods are near BenchmarkDotNet's overhead-subtraction/noise region, and the
original distribution was multimodal. The **+0.280 ns absolute difference** and
assembly are more useful than the percentage. A separate ShortRun used for
disassembly reproduced the direction: 2.060 vs 2.569 ns.

### Assembly Findings

The original benchmark body is **9 B**: load the VM field, load the `Zones` backing
field, and return. The virtual benchmark body is **35 B**.

The original `Zones` path is conceptually:

```text
Load VM -> load Zones field -> return
```

to:

```text
Load VM -> check exact VM type -> load Zones field -> return
otherwise call the virtual getter
```

This is **guarded devirtualization**: the exact expected type takes a direct field-load
path; a subclass falls back to its vtable slot. The fixture takes the guarded fast path.

For this one-layer property and JIT run, the virtual fast path adds one method-table
comparison and branch. The approximately 0.28 ns absolute cost is much smaller than
the previous four-layer flattened experiment. Hardware counters were not collected.

## Serialization and Deserialization Tests

`VirtualMachineSerializationBenchmarks.cs` calls the actual generated SDK paths:

```csharp
ModelReaderWriter.Write(model, options, sdkContext);
ModelReaderWriter.Read<VirtualMachineData>(response, options, sdkContext);
```

Each assembly uses its own generated context. Setup verifies matching serialized
bytes, parsed nested fields, read-only visibility and round trips. Deserialization
receives response-only fields in both modes.

- `J`: JSON persistence representation, including response-only model data.
- `W`: service wire rules, used when SDK operations create request content. It still
  uses JSON encoding here. `GetFormatFromOptions()` resolves a local format variable
  to `J`; it does **not** change `options.Format`, which remains `W` for field filtering.

Mean elapsed times per VM; positive delta means slower for the virtual clone:

| Operation | Format | Run 1 original | Run 1 virtual | Delta | Run 2 original | Run 2 virtual | Delta |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Serialize | J | 5.843 us | 6.016 us | +3.0% | 5.629 us | 6.069 us | +7.8% |
| Serialize | W | 3.648 us | 3.900 us | +6.9% | 3.611 us | 3.820 us | +5.8% |
| Deserialize | J | 16.807 us | 16.344 us | -2.8% | 16.570 us | 16.239 us | -2.0% |
| Deserialize | W | 16.345 us | 16.390 us | +0.3% | 16.571 us | 16.332 us | -1.4% |

Run 2's 99.9% confidence-interval half-widths were 0.0753/0.0886 us for J writes,
0.0321/0.0300 us for W writes, 0.1845/0.0863 us for J reads, and 0.1222/0.1186 us
for W reads (original/virtual).

Run 2 reported equal allocations: 4,384 B for J serialization, 2,192 B for W
serialization, and 9,200 B for deserialization. The first run's virtual J allocation
entry was inconsistent and must not be interpreted as an allocation saving.

### Brief Interpretation

**Serialization:** the W slowdown reproduced at approximately 5.8-6.9%, an additional
0.209-0.252 us per VM. The writer repeatedly accesses the internal properties object
and nested models. Separate disassembly showed added accessor type guards and larger
VM writer bodies: 6,731 to 7,363 B for J, and 6,566 to 7,086 B for W. This supports a
real optimization/call-path cost, without proving an exact cycle breakdown.

**Deserialization:** JSON is parsed into locals, then concrete models are constructed.
It does not populate the graph by traversing the public flattened getters. Captured
code showed some constructors inlined into direct stores and others using guarded
direct-store paths, avoiding much of the potential virtual-setter overhead.
There were also favorable builder/create specializations in some virtual entry paths.
These findings explain why read and write performance need not move together, but
do not prove that virtual inherently accelerates parsing. W deserialization's
direction changed between runs; J had slightly lower means in both runs.

## Reproduction and Limits

The attached benchmark files are reviewable source, not a standalone distribution
of the two SDKs. They require the experimental aliased project references, generated
contexts, and a copied-to-output `VirtualMachine.json`. The clone and transformation
tool are in the working tree under `sdk/compute/Azure.ResourceManager.Compute.Virtual`;
this Gist does not publish the entire SDK copy or imply it has been merged upstream.

From that package's `perf` directory:

```powershell
dotnet run -c Release -f net10.0 -- --validate
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachinePropertyReadBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachineSerializationBenchmarks*" --launchCount 2 --warmupCount 5 --iterationCount 15
dotnet run -c Release -f net10.0 -- --filter "*VirtualMachinePropertyReadBenchmarks*" --job short --disasm --disasmDepth 2
```

This is one VM object shape on one virtualized machine/runtime. The first J write
baseline was bimodal; process/code layout and profile-sensitive JIT decisions remain
contributors. Diagnostic assembly was captured in separate runs. No cold-start, AOT,
live-service latency or Moq overhead was measured. Repeat on idle representative
hardware and additional payloads before using the results as an SDK-wide threshold.

**Takeaway:** virtual properties offer a useful read-only mocking experience, but
they are not performance-neutral. This experiment shows a noticeable cost in
property-intensive reads and a smaller repeated serialization cost, without a
consistent deserialization regression.
