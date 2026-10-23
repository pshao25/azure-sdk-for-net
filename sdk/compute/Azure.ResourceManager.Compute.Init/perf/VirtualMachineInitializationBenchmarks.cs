// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Initialized;

using BenchmarkDotNet.Attributes;
using InitVm = Initialized::Azure.ResourceManager.Compute.VirtualMachineData;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;

namespace Compute.Init.Perf;

[MemoryDiagnoser]
public class VirtualMachineInitializationBenchmarks
{
    [Benchmark(Baseline = true)]
    public object ConstructOriginal() => new OriginalVm(Azure.Core.AzureLocation.WestUS2);

    [Benchmark]
    public object ConstructInitialized() => new InitVm(Azure.Core.AzureLocation.WestUS2);

    [Benchmark]
    public object ConstructWithOutputProperties() => new InitVm(Azure.Core.AzureLocation.WestUS2)
    {
        ETag = "perf-etag",
        ManagedBy = "test-manager",
        ProvisioningState = "Succeeded",
        VmId = "11111111-1111-1111-1111-111111111111",
        TimeCreated = new DateTimeOffset(2026, 1, 2, 3, 4, 5, TimeSpan.Zero),
        Zones = ["1"]
    };
}
