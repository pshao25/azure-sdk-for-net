// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Generic;

using System.ClientModel.Primitives;
using GenericFactory = Generic::Azure.ResourceManager.Compute.ModelFactory;
using GenericVm = Generic::Azure.ResourceManager.Compute.VirtualMachineData;
using GenericContext = Generic::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;

namespace Compute.Generic.Perf;

internal static class GenericModelFactoryTests
{
    public static void Validate()
    {
        var zones = new List<string> { "1", "2" };
        GenericVm baseline = GenericFactory.Create<GenericVm>(new
        {
            Name = "vm1",
            Location = Azure.Core.AzureLocation.WestUS2,
            ProvisioningState = "Succeeded",
            ETag = "etag-1",
            Zones = zones,
            OSProfile = new
            {
                ComputerName = "test-computer",
                AdminUsername = "test-user"
            }
        });

        if (baseline.Name != "vm1" || baseline.Location != Azure.Core.AzureLocation.WestUS2
            || baseline.ProvisioningState != "Succeeded" || baseline.ETag != "etag-1"
            || baseline.Zones.Count != 2 || baseline.OSProfile.ComputerName != "test-computer")
        {
            throw new InvalidOperationException("Generic property mapping failed.");
        }

        GenericVm failed = GenericFactory.Create(new { ProvisioningState = "Failed", ETag = (string?)null }, baseline);
        if (ReferenceEquals(failed, baseline) || failed.ProvisioningState != "Failed" || failed.ETag is not null
            || baseline.ProvisioningState != "Succeeded" || baseline.ETag != "etag-1"
            || !ReferenceEquals(failed.Zones, baseline.Zones) || !ReferenceEquals(failed.OSProfile, baseline.OSProfile))
        {
            throw new InvalidOperationException("Current must be a shallow-copy template and remain unchanged.");
        }

        AssertArgumentError(() => GenericFactory.Create<GenericVm>(new { ProvisioningStatus = "Succeeded" }), "ProvisioningStatus");
        AssertArgumentError(() => GenericFactory.Create<GenericVm>(new { ProvisioningState = 123 }), "ProvisioningState");

        BinaryData genericJson = ModelReaderWriter.Write(baseline, new ModelReaderWriterOptions("J"), GenericContext.Default);
        GenericVm roundTrip = ModelReaderWriter.Read<GenericVm>(genericJson, new ModelReaderWriterOptions("J"), GenericContext.Default)!;
        if (roundTrip.Name != baseline.Name || roundTrip.ProvisioningState != baseline.ProvisioningState
            || roundTrip.OSProfile.ComputerName != baseline.OSProfile.ComputerName)
        {
            throw new InvalidOperationException("Generic model did not round-trip.");
        }
    }

    private static void AssertArgumentError(Action action, string propertyName)
    {
        try
        {
            action();
            throw new InvalidOperationException($"Expected invalid property '{propertyName}' to fail.");
        }
        catch (ArgumentException ex) when (ex.Message.Contains(propertyName, StringComparison.Ordinal))
        {
        }
    }
}
