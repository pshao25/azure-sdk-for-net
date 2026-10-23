// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

extern alias Original;
extern alias Initialized;

using System.ClientModel.Primitives;
using System.Reflection;
using System.Text.Json;
using InitVm = Initialized::Azure.ResourceManager.Compute.VirtualMachineData;
using InitExtension = Initialized::Azure.ResourceManager.Compute.VirtualMachineExtensionData;
using InitContext = Initialized::Azure.ResourceManager.Compute.AzureResourceManagerComputeContext;
using OriginalVm = Original::Azure.ResourceManager.Compute.VirtualMachineData;

namespace Compute.Init.Perf;

internal static class VirtualMachineInitTests
{
    private static readonly string[] InitPropertyNames =
    [
        "Resources", "Zones", "ManagedBy", "ETag",
        "ProvisioningState", "InstanceView", "VmId", "TimeCreated"
    ];

    public static void Validate()
    {
        ValidateAccessorShapes();

        DateTimeOffset createdOn = new(2026, 1, 2, 3, 4, 5, TimeSpan.Zero);
        var vm = new InitVm(Azure.Core.AzureLocation.WestUS2)
        {
            Resources = Array.Empty<InitExtension>(),
            Zones = ["1", "2"],
            ManagedBy = "test-manager",
            ETag = "test-etag",
            ProvisioningState = "Succeeded",
            VmId = "11111111-1111-1111-1111-111111111111",
            TimeCreated = createdOn
        };

        if (vm.Resources.Count != 0 || vm.Zones.Count != 2
            || vm.ManagedBy != "test-manager" || vm.ETag != "test-etag"
            || vm.ProvisioningState != "Succeeded"
            || vm.VmId != "11111111-1111-1111-1111-111111111111"
            || vm.TimeCreated != createdOn)
        {
            throw new InvalidOperationException("Object initializer did not preserve init-only values.");
        }

        // Collection contents remain mutable after initialization; init controls only
        // replacement of the property reference during construction.
        vm.Zones.Add("3");
        if (vm.Zones.Count != 3)
        {
            throw new InvalidOperationException("Zones collection behavior changed unexpectedly.");
        }

        using JsonDocument json = JsonDocument.Parse(ModelReaderWriter.Write(vm, new ModelReaderWriterOptions("J"), InitContext.Default));
        using JsonDocument wire = JsonDocument.Parse(ModelReaderWriter.Write(vm, new ModelReaderWriterOptions("W"), InitContext.Default));
        JsonElement jsonProperties = json.RootElement.GetProperty("properties");
        JsonElement wireProperties = wire.RootElement.GetProperty("properties");
        if (json.RootElement.GetProperty("etag").GetString() != "test-etag"
            || json.RootElement.GetProperty("managedBy").GetString() != "test-manager"
            || jsonProperties.GetProperty("provisioningState").GetString() != "Succeeded"
            || jsonProperties.GetProperty("vmId").GetString() != vm.VmId
            || !jsonProperties.TryGetProperty("timeCreated", out _)
            || wire.RootElement.TryGetProperty("etag", out _)
            || wire.RootElement.TryGetProperty("managedBy", out _)
            || wireProperties.TryGetProperty("provisioningState", out _)
            || wireProperties.TryGetProperty("vmId", out _)
            || wireProperties.TryGetProperty("timeCreated", out _)
            || wire.RootElement.GetProperty("zones").GetArrayLength() != 3)
        {
            throw new InvalidOperationException("Init accessors changed J/W visibility semantics.");
        }
    }

    private static void ValidateAccessorShapes()
    {
        foreach (string name in InitPropertyNames)
        {
            PropertyInfo original = typeof(OriginalVm).GetProperty(name)!;
            PropertyInfo initialized = typeof(InitVm).GetProperty(name)!;
            if (original.SetMethod is not null || initialized.SetMethod is null
                || initialized.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                    .All(type => type.FullName != "System.Runtime.CompilerServices.IsExternalInit"))
            {
                throw new InvalidOperationException($"Unexpected init accessor shape for {name}.");
            }
        }

        PropertyInfo properties = typeof(InitVm).GetProperty("Properties", BindingFlags.NonPublic | BindingFlags.Instance)!;
        if (properties.SetMethod is null
            || properties.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                .Any(type => type.FullName == "System.Runtime.CompilerServices.IsExternalInit"))
        {
            throw new InvalidOperationException("Internal Properties must retain an ordinary setter.");
        }

        Type internalPropertiesType = typeof(InitVm).Assembly.GetType("Azure.ResourceManager.Compute.Models.VirtualMachineProperties")!;
        foreach (string name in new[] { "ProvisioningState", "InstanceView", "VmId", "TimeCreated" })
        {
            PropertyInfo property = internalPropertiesType.GetProperty(name)!;
            if (property.SetMethod is null
                || property.SetMethod.ReturnParameter.GetRequiredCustomModifiers()
                    .Any(type => type.FullName == "System.Runtime.CompilerServices.IsExternalInit"))
            {
                throw new InvalidOperationException($"Internal backing property {name} must retain an ordinary setter.");
            }
        }
    }
}
