# Compute Virtual Experiment

The real Compute `VirtualMachineData` serialization/deserialization comparison is
in [perf](perf/README.md), using the SDK's generated J/W model reader/writer paths.

Nonshipping, nonpackable source clone of the sibling `Azure.ResourceManager.Compute` SDK. The assembly and package identity are `Azure.ResourceManager.Compute.Virtual`; all source namespaces remain unchanged. Do not reference both assemblies without assembly aliases, or expose their identically named types interchangeably.

## Reproduce

Run from the repository root with its .NET 10.0.400 SDK. These commands use the locally installed SDK in this workspace:

```powershell
$env:PATH = "C:\Users\pashao\AppData\Local\Temp\opencode\dotnet-10.0.400;" + $env:PATH
$clone = "sdk/compute/Azure.ResourceManager.Compute.Virtual"
$tool = "$clone/tools/Virtualize.csproj"
dotnet run --project $tool -c Release -- --self-test
dotnet run --project $tool -c Release -- $clone
dotnet run --project $tool -c Release --no-build -- $clone --verify
dotnet build sdk/compute/Azure.ResourceManager.Compute/src/Azure.ResourceManager.Compute.csproj -c Release -f net10.0
dotnet build "$clone/src/Azure.ResourceManager.Compute.Virtual.csproj" -c Release -f net10.0
```

The standalone tool references Roslyn shipped with the selected SDK, without adding NuGet dependencies. Its own `Directory.Build.props` sets `IsToolProject` before importing the parent. The package-level props set only nonshipping/nonpackable classification before importing repository settings, so they do not reclassify projects under `perf/` as tools.

Never manually edit the cloned source or manifest. Change the tool and run it again. This transformation is the clone's generator; it does not run TypeSpec/AutoRest or regenerate the original SDK. The clone project file is deliberately maintained separately: when the original project changes, review version, flags and build settings here too. It currently preserves version `1.17.0-beta.1`, inherited `net10.0;net8.0;netstandard2.0` frameworks, runtime dependencies, management shared-source imports, signing and `DisableEnhancedAnalysis`. There is no published virtual package API baseline, so the clone has no `ApiCompatVersion`. The original retains its normal API compatibility checks.

## Transformation Contract

- Copy every file under the original `src/`, including non-C# assets, with the same relative path, excluding project files and `bin/`/`obj/`.
- Parse C# 14 with Roslyn and inspect class properties and indexers, not only public models. Internal `FooProperties` serialization models, resources, REST clients and internal helper classes are included.
- Add `virtual ` before the property type for public, protected, internal, protected internal and private protected instance properties, including properties with private setters.
- Leave private/default-private, static, already virtual, override, abstract, sealed, explicit-interface and ref-return properties unchanged. Do not virtualize properties on structs, record structs, interfaces, sealed classes or static classes. Record-class explicit properties are supported; synthesized properties are outside source transformation scope.
- Identify sealed/static types across partial declarations using lexical namespaces, containing types and generic arity. Nested types are evaluated independently of their containers. Exhaustively parse conditional symbol combinations (up to eight symbols); conflicting eligibility fails rather than silently producing a target-specific clone.
- Preserve source bytes except inserted ASCII `virtual ` text, including comments, whitespace, line endings and UTF-8 BOMs. Invalid/non-UTF-8 C# input fails rather than being silently re-encoded. Non-C# assets are copied verbatim.
- Leave externally linked repository shared sources and dependency assemblies untouched. Their properties, inherited members defined outside the original `src/`, and build-generated assembly metadata are not clone transformation inputs.
- Do not copy tests, recordings, CI or packaging infrastructure. Never access `perf/` during generation.

`virtualization-manifest.json` records the original project hash, each input/output SHA-256 hash, every changed property name, every skipped property and mutually exclusive skipped-reason counts. It has stable ordering, no timestamp and no machine-specific absolute paths. Hashes intentionally cover exact checkout bytes, so changing checkout line endings requires regeneration.

`--verify` reconstructs the expected outputs and manifest in memory, compares exact bytes and file sets, and exits nonzero on missing, edited, stale or extra files without writing clone files. Use `--no-build` as above to avoid tool build writes as well. Each transformation reverses its insertions in memory to prove all other characters are unchanged. Regeneration removes obsolete source files only when the previous manifest proves they are unedited generator outputs; unknown or edited extra files cause a failure.

## Validated Snapshot

Generated and verified **1,778 files**, adding `virtual` to **2,954 properties** in **672 files**, including the two change-tracking collection indexers. The remaining **1,106 files** are byte-identical, including all **594 `*.Serialization.cs` files**. Both Release `net10.0` builds passed with zero warnings and zero errors; the original also passed its normal API compatibility check. Self-tests cover eligibility, partial sealing, generic/nested identity, records, indexers, ref returns, conditional branches, trivia preservation, idempotency and Roslyn compilation of transformed examples in both conditional configurations.

| Exclusion | Property Count |
| --- | ---: |
| Already virtual / override / abstract / sealed | 163 |
| Explicit interface | 2 |
| Non-class (primarily extensible-enum value types) | 539 |
| Private / implicit private | 66 |
| Sealed or static containing type | 1 |
| Static property on other classes | 43 |
| Ref return | 0 |

The one static-type exclusion is `ProviderConstants.DefaultProviderNamespace`. The two explicit-interface exclusions are `ChangeTrackingDictionary<TKey, TValue>`'s `IReadOnlyDictionary<TKey, TValue>.Keys` and `.Values`. Exact names and file locations for all exceptions are in the manifest.

This is a source-level virtual-dispatch experiment, not a supported replacement package. Constructors and serialization continue calling the same properties, which now permit overrides; that behavior change is intentional. No live-service tests or performance measurements are run by this tool. Only `net10.0` was build-validated for this snapshot; the other inherited frameworks remain configured.
