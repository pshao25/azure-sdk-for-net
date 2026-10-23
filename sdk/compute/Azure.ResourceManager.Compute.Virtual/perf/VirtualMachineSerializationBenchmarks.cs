// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Virtualized;

using Azure.ResourceManager.Models;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using System.ClientModel.Primitives;
using System.Reflection;
using System.Text.Json;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;
using VirtualVm = Virtualized::Azure.ResourceManager.Compute.VirtualMachineData;
using OriginalContext = Original::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;
using VirtualContext = Virtualized::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;

namespace Compute.Virtual.Perf;

[MemoryDiagnoser]
[CategoriesColumn]
[GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
public class VirtualMachineSerializationBenchmarks
{
    private BinaryData _response = null!;
    private ModelReaderWriterOptions _options = null!;
    private OriginalVm _original = null!;
    private VirtualVm _virtual = null!;

    [Params("J", "W")]
    public string Format { get; set; } = "J";

    [GlobalSetup]
    public void Setup()
    {
        _options = new ModelReaderWriterOptions(Format);
        _response = new BinaryData(File.ReadAllBytes(Path.Combine(AppContext.BaseDirectory, "VirtualMachine.json")));
        if (typeof(OriginalVm).BaseType != typeof(TrackedResourceData)
            || typeof(VirtualVm).BaseType != typeof(TrackedResourceData)
            || typeof(OriginalVm).Assembly == typeof(VirtualVm).Assembly)
        {
            throw new InvalidOperationException("Expected two distinct Compute assemblies with TrackedResourceData models.");
        }
        foreach (var type in new[] { typeof(OriginalVm), typeof(VirtualVm) })
        {
            bool expectedVirtual = type == typeof(VirtualVm);
            foreach (string name in new[] { "HardwareProfile", "StorageProfile", "ProvisioningState", "Properties" })
            {
                var getter = type.GetProperty(name, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)!.GetMethod!;
                if (getter.IsVirtual != expectedVirtual)
                {
                    throw new InvalidOperationException($"Unexpected virtual modifier: {type.Assembly.GetName().Name}.{name}");
                }
            }
            var properties = type.Assembly.GetType("Azure.ResourceManager.Compute.Models.VirtualMachineProperties")!;
            if (properties.GetProperty("StorageProfile")!.GetMethod!.IsVirtual != expectedVirtual)
            {
                throw new InvalidOperationException("The internal serialization model must also be virtualized.");
            }
        }

        _original = ModelReaderWriter.Read<OriginalVm>(_response, _options, OriginalContext.Default)!;
        _virtual = ModelReaderWriter.Read<VirtualVm>(_response, _options, VirtualContext.Default)!;
        if (_original.Name != "perf-vm" || _virtual.Name != "perf-vm"
            || _original.Location.ToString() != "westus2" || _virtual.Location.ToString() != "westus2"
            || _original.ETag != "perf-etag" || _virtual.ETag != "perf-etag"
            || _original.Id != _virtual.Id || _original.Tags.Count != 3 || _virtual.Tags.Count != 3
            || _original.HardwareProfile.VmSize.ToString() != "Standard_D4s_v5"
            || _virtual.HardwareProfile.VmSize.ToString() != "Standard_D4s_v5"
            || _original.StorageProfile.DataDisks.Count != 2 || _virtual.StorageProfile.DataDisks.Count != 2
            || _original.StorageProfile.OSDisk.DiskSizeGB != 128 || _virtual.StorageProfile.OSDisk.DiskSizeGB != 128
            || _original.NetworkProfile.NetworkInterfaces.Count != 2 || _virtual.NetworkProfile.NetworkInterfaces.Count != 2
            || _original.OSProfile.LinuxConfiguration.DisablePasswordAuthentication != true
            || _virtual.OSProfile.LinuxConfiguration.DisablePasswordAuthentication != true
            || _original.ProvisioningState != "Succeeded" || _virtual.ProvisioningState != "Succeeded"
            || _original.InstanceView.Statuses.Count != 2 || _virtual.InstanceView.Statuses.Count != 2
            || _original.InstanceView.Disks.Count != 1 || _virtual.InstanceView.Disks.Count != 1
            || _original.Zones.Count != 1 || _virtual.Zones.Count != 1
            || _original.VmId != "11111111-1111-1111-1111-111111111111" || _original.VmId != _virtual.VmId)
        {
            throw new InvalidOperationException("VM response data was not restored correctly.");
        }

        BinaryData originalJson = SerializeOriginal();
        BinaryData virtualJson = SerializeVirtual();
        if (!originalJson.ToMemory().Span.SequenceEqual(virtualJson.ToMemory().Span))
        {
            throw new InvalidOperationException("Original and virtual Compute models must serialize identically.");
        }
        using var json = JsonDocument.Parse(virtualJson);
        var root = json.RootElement;
        var propertiesJson = root.GetProperty("properties");
        if (propertiesJson.TryGetProperty("provisioningState", out _) != (Format == "J")
            || propertiesJson.TryGetProperty("instanceView", out _) != (Format == "J")
            || propertiesJson.TryGetProperty("vmId", out _) != (Format == "J")
            || root.GetProperty("tags").GetProperty("environment").GetString() != "benchmark"
            || propertiesJson.GetProperty("storageProfile").GetProperty("dataDisks").GetArrayLength() != 2)
        {
            throw new InvalidOperationException("Read-only wire visibility and nested request fields must be preserved.");
        }
        var originalRoundTrip = ModelReaderWriter.Read<OriginalVm>(originalJson, _options, OriginalContext.Default)!;
        var virtualRoundTrip = ModelReaderWriter.Read<VirtualVm>(virtualJson, _options, VirtualContext.Default)!;
        if (!originalJson.ToMemory().Span.SequenceEqual(ModelReaderWriter.Write(originalRoundTrip, _options, OriginalContext.Default).ToMemory().Span)
            || !virtualJson.ToMemory().Span.SequenceEqual(ModelReaderWriter.Write(virtualRoundTrip, _options, VirtualContext.Default).ToMemory().Span))
        {
            throw new InvalidOperationException("VM data must round-trip in the selected format.");
        }
    }

    [Benchmark(Baseline = true), BenchmarkCategory("Serialize")]
    public BinaryData SerializeOriginal() => ModelReaderWriter.Write(_original, _options, OriginalContext.Default);

    [Benchmark, BenchmarkCategory("Serialize")]
    public BinaryData SerializeVirtual() => ModelReaderWriter.Write(_virtual, _options, VirtualContext.Default);

    // The response includes read-only fields in both formats. Returning object avoids aliased
    // SDK type names in BenchmarkDotNet's generated harness; reference conversion does not box.
    [Benchmark(Baseline = true), BenchmarkCategory("Deserialize")]
    public object DeserializeOriginal() => ModelReaderWriter.Read<OriginalVm>(_response, _options, OriginalContext.Default)!;

    [Benchmark, BenchmarkCategory("Deserialize")]
    public object DeserializeVirtual() => ModelReaderWriter.Read<VirtualVm>(_response, _options, VirtualContext.Default)!;
}
