// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Initialized;

using Azure.ResourceManager.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.ClientModel.Primitives;
using System.Text.Json;
using InitVm = Initialized::Azure.ResourceManager.Compute.VirtualMachineData;
using InitContext = Initialized::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;
using OriginalContext = Original::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;

namespace Compute.Init.Perf;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class VirtualMachineSerializationBenchmarks
{
    private BinaryData _response = null!;
    private ModelReaderWriterOptions _options = null!;
    private OriginalVm _original = null!;
    private InitVm _initialized = null!;

    [Params("J", "W")]
    public string Format { get; set; } = "J";

    [GlobalSetup]
    public void Setup()
    {
        _options = new ModelReaderWriterOptions(Format);
        _response = new BinaryData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "VirtualMachine.json")));
        if (typeof(OriginalVm).BaseType != typeof(TrackedResourceData)
            || typeof(InitVm).BaseType != typeof(TrackedResourceData)
            || typeof(OriginalVm).Assembly == typeof(InitVm).Assembly)
        {
            throw new InvalidOperationException("Expected distinct original and init Compute SDK assemblies.");
        }

        _original = ModelReaderWriter.Read<OriginalVm>(_response, _options, OriginalContext.Default)!;
        _initialized = ModelReaderWriter.Read<InitVm>(_response, _options, InitContext.Default)!;
        if (_original.Name != "perf-vm" || _initialized.Name != "perf-vm"
            || _original.ETag != "perf-etag" || _initialized.ETag != "perf-etag"
            || _original.ProvisioningState != "Succeeded" || _initialized.ProvisioningState != "Succeeded"
            || _original.VmId != _initialized.VmId
            || _original.StorageProfile.DataDisks.Count != 2 || _initialized.StorageProfile.DataDisks.Count != 2
            || _original.NetworkProfile.NetworkInterfaces.Count != 2 || _initialized.NetworkProfile.NetworkInterfaces.Count != 2
            || _original.InstanceView.Statuses.Count != 2 || _initialized.InstanceView.Statuses.Count != 2)
        {
            throw new InvalidOperationException("VM response data differs between assemblies.");
        }

        BinaryData originalJson = SerializeOriginal();
        BinaryData initJson = SerializeInitialized();
        if (!originalJson.ToMemory().Span.SequenceEqual(initJson.ToMemory().Span))
        {
            throw new InvalidOperationException("Original and init models must serialize identically.");
        }

        using JsonDocument json = JsonDocument.Parse(initJson);
        JsonElement properties = json.RootElement.GetProperty("properties");
        if (properties.TryGetProperty("provisioningState", out _) != (Format == "J")
            || properties.TryGetProperty("instanceView", out _) != (Format == "J")
            || properties.TryGetProperty("vmId", out _) != (Format == "J")
            || json.RootElement.TryGetProperty("etag", out _) != (Format == "J"))
        {
            throw new InvalidOperationException("Read-only J/W visibility changed.");
        }

        var originalRoundTrip = ModelReaderWriter.Read<OriginalVm>(originalJson, _options, OriginalContext.Default)!;
        var initRoundTrip = ModelReaderWriter.Read<InitVm>(initJson, _options, InitContext.Default)!;
        if (!originalJson.ToMemory().Span.SequenceEqual(ModelReaderWriter.Write(originalRoundTrip, _options, OriginalContext.Default).ToMemory().Span)
            || !initJson.ToMemory().Span.SequenceEqual(ModelReaderWriter.Write(initRoundTrip, _options, InitContext.Default).ToMemory().Span))
        {
            throw new InvalidOperationException("VM data must round-trip in the selected format.");
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Serialize")]
    public BinaryData SerializeOriginal() => ModelReaderWriter.Write(_original, _options, OriginalContext.Default);

    [Benchmark, BenchmarkCategory("Serialize")]
    public BinaryData SerializeInitialized() => ModelReaderWriter.Write(_initialized, _options, InitContext.Default);

    [Benchmark(Baseline = true), BenchmarkCategory("Deserialize")]
    public object DeserializeOriginal() => ModelReaderWriter.Read<OriginalVm>(_response, _options, OriginalContext.Default)!;

    [Benchmark, BenchmarkCategory("Deserialize")]
    public object DeserializeInitialized() => ModelReaderWriter.Read<InitVm>(_response, _options, InitContext.Default)!;
}
