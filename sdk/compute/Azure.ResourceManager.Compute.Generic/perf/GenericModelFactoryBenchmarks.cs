// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Generic;

using BenchmarkDotNet.Attributes;
using GenericModelFactory = Generic::Azure.ResourceManager.Compute.ModelFactory;
using GenericVm = Generic::Azure.ResourceManager.Compute.VirtualMachineData;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;

namespace Compute.Generic.Perf;

[MemoryDiagnoser]
public class GenericModelFactoryBenchmarks
{
    private GenericVm _baseline = null!;

    [GlobalSetup]
    public void Setup()
    {
        _baseline = GenericModelFactory.Create<GenericVm>(new
        {
            Name = "vm1",
            Location = Azure.Core.AzureLocation.WestUS2,
            ProvisioningState = "Succeeded",
            ETag = "etag-1",
            Zones = new[] { "1" }
        });
    }

    [Benchmark(Baseline = true)]
    public object PublicConstructor() => new OriginalVm(Azure.Core.AzureLocation.WestUS2);

    [Benchmark]
    public object GenericCreate() => GenericModelFactory.Create<GenericVm>(new
    {
        Name = "vm1",
        Location = Azure.Core.AzureLocation.WestUS2,
        ProvisioningState = "Succeeded",
        ETag = "etag-1",
        Zones = new[] { "1" }
    });

    [Benchmark]
    public object GenericCreateWithCurrent() => GenericModelFactory.Create(
        new { ProvisioningState = "Failed" }, _baseline);
}
