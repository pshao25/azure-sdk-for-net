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

        var originalGetter = typeof(OriginalVm).GetProperty(nameof(OriginalVm.Zones))!.GetMethod!;
        var virtualGetter = typeof(VirtualVm).GetProperty(nameof(VirtualVm.Zones))!.GetMethod!;
        if (originalGetter.IsVirtual || !virtualGetter.IsVirtual || virtualGetter.IsFinal)
        {
            throw new InvalidOperationException("Zones does not have the expected virtual modifiers.");
        }

        _original = ModelReaderWriter.Read<OriginalVm>(response, options, OriginalContext.Default)!;
        _virtual = ModelReaderWriter.Read<VirtualVm>(response, options, VirtualContext.Default)!;

        if (ReadZonesOriginal().Count != 1 || ReadZonesVirtual().Count != 1)
        {
            throw new InvalidOperationException("Zones was not initialized correctly.");
        }
    }

    [Benchmark(Baseline = true)]
    public IList<string> ReadZonesOriginal() => _original.Zones;

    [Benchmark]
    public IList<string> ReadZonesVirtual() => _virtual.Zones;
}
