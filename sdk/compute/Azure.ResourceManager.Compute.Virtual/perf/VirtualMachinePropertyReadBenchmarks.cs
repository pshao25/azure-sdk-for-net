// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Virtualized;

using BenchmarkDotNet.Attributes;
using System.ClientModel.Primitives;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;
using VirtualVm = Virtualized::Azure.ResourceManager.Compute.VirtualMachineData;
using OriginalContext = Original::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;
using VirtualContext = Virtualized::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;

namespace Compute.Virtual.Perf;

[MemoryDiagnoser]
public class VirtualMachinePropertyReadBenchmarks
{
    private OriginalVm _original = null!;
    private VirtualVm _virtual = null!;

    [GlobalSetup]
    public void Setup()
    {
        var response = new BinaryData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "VirtualMachine.json")));
        var options = new ModelReaderWriterOptions("W");

        var originalGetter = typeof(OriginalVm).GetProperty(nameof(OriginalVm.ETag))!.GetMethod!;
        var virtualGetter = typeof(VirtualVm).GetProperty(nameof(VirtualVm.ETag))!.GetMethod!;
        if (originalGetter.IsVirtual || !virtualGetter.IsVirtual || virtualGetter.IsFinal)
        {
            throw new InvalidOperationException("ETag does not have the expected virtual modifiers.");
        }

        _original = ModelReaderWriter.Read<OriginalVm>(response, options, OriginalContext.Default)!;
        _virtual = ModelReaderWriter.Read<VirtualVm>(response, options, VirtualContext.Default)!;

        if (ReadETagOriginal() != "perf-etag" || ReadETagVirtual() != "perf-etag")
        {
            throw new InvalidOperationException("ETag was not initialized correctly.");
        }
    }

    [Benchmark(Baseline = true)]
    public string ReadETagOriginal() => _original.ETag;

    [Benchmark]
    public string ReadETagVirtual() => _virtual.ETag;
}
