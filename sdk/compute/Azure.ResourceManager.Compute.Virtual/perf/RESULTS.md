# Compute Virtual Property Results

## Scope And Verification

Measured the real `Azure.ResourceManager.Compute` 1.17.0-beta.1 source snapshot
against its sibling virtualized clone on 2026-09-16. The source generator copied
1,778 files and inserted `virtual` for 2,954 eligible properties in 672 files.
All 594 serialization files remain byte-identical. The transformation manifest
records exact source/output hashes and exclusions. The original SDK was not changed.

Both SDKs built in Release/net10.0 with zero warnings/errors; original API checks
were retained. Transformation self-tests and byte-for-byte `--verify` passed.
Benchmark `--validate` passed for both J and W: inheritance, public/internal virtual
flags, nested response values, identical output bytes, read-only visibility and
round trips. No service requests were made.

The input is the permanent synthetic response fixture `VirtualMachine.json`, not
a live recording. It uses the actual `VirtualMachineData` model and includes OS,
storage, image, two data disks, two NICs, tags, security, diagnostics, and instance
view/statuses. Timed calls use each SDK's generated `ModelReaderWriter` context.
The clone changes internal wrappers and helper collection properties too, so this
measures the complete requested property policy rather than public getters alone.

## Environment And Runs

- BenchmarkDotNet 0.15.8; .NET SDK 10.0.400; runtime 10.0.11, X64 RyuJIT.
- Windows 11 / Hyper-V; AMD EPYC 7763, 16 logical / 8 physical cores exposed.
- Concurrent Server GC; normal JIT tiering/inlining/devirtualization/PGO defaults.
- Two independent complete runs, each with two process launches per scenario,
  five warmups and 15 measurement iterations per launch. All eight scenarios passed
  in each run. No CPU-intensive build/review commands were run concurrently.

From the perf directory:

```powershell
dotnet run -c Release -f net10.0 -- --filter "*" --launchCount 2 --warmupCount 5 --iterationCount 15
dotnet run -c Release -f net10.0 -- --filter "*" --launchCount 2 --warmupCount 5 --iterationCount 15 --artifacts "BenchmarkDotNet.Artifacts/repeat"
```

## Timing Results

Mean times per VM; positive delta means greater elapsed time for the virtual clone.
Percentages are calculated from the displayed means, not rounded BDN ratios.

| Operation | Format | Run 1 original | Run 1 virtual | Run 1 delta | Run 2 original | Run 2 virtual | Run 2 delta |
| --- | --- | ---: | ---: | ---: | ---: | ---: | ---: |
| Serialize | J | 5.843 us | 6.016 us | +3.0% | 5.629 us | 6.069 us | +7.8% |
| Serialize | W | 3.648 us | 3.900 us | +6.9% | 3.611 us | 3.820 us | +5.8% |
| Deserialize | J | 16.807 us | 16.344 us | -2.8% | 16.570 us | 16.239 us | -2.0% |
| Deserialize | W | 16.345 us | 16.390 us | +0.3% | 16.571 us | 16.332 us | -1.4% |

Run 2's 99.9% confidence-interval half-widths (original / virtual):

| Operation | Format | Original error | Virtual error |
| --- | --- | ---: | ---: |
| Serialize | J | 0.0753 us | 0.0886 us |
| Serialize | W | 0.0321 us | 0.0300 us |
| Deserialize | J | 0.1845 us | 0.0863 us |
| Deserialize | W | 0.1222 us | 0.1186 us |

The W serialization slowdown reproduced in both runs, with an absolute increase
of 0.209-0.252 us per VM. J serialization also had higher means in both runs but a
more variable magnitude; the first original J distribution was bimodal.
Deserialization did not show a consistent slowdown. Lower J deserialization means
are observations, not proof that virtual intrinsically improves parsing.

## Allocations And Limits

Run 2 reported matching allocations in both variants: 4,384 B for J serialization,
2,192 B for W serialization, and 9,200 B for deserialization in either format.
The first run had an invalid zero/missing allocation measurement for virtual J
serialization; it is not evidence of savings. The repeat did not show that anomaly.

These results cover one populated VM shape on one virtual machine/runtime, not all
Compute resources or the full HTTP pipeline. Outliers were removed by BenchmarkDotNet;
the raw logs retain measurements. There was no disassembly/profiling of Compute
hotspots, so no exact JIT-level cause is claimed. Source identities, process/code
layout and environmental variability remain possible contributors. Repeat on idle
representative physical hardware and additional VM shapes before choosing an SDK-wide
performance threshold. No cold-start, AOT, or live-service behavior was measured.

Raw reports: `BenchmarkDotNet.Artifacts/results/` and
`BenchmarkDotNet.Artifacts/repeat/results/`, file prefix
`Compute.Virtual.Perf.VirtualMachineSerializationBenchmarks-report` (CSV/Markdown/HTML).
Those local artifacts are ignored; this report and the fixture remain in source.
