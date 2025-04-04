// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.MachineLearning.Models;
using NUnit.Framework;

namespace Azure.ResourceManager.MachineLearning.Samples
{
    public partial class Sample_MachineLearningRegistryEnvironmentContainerResource
    {
        [Test]
        [Ignore("Only validating compilation of examples")]
        public async Task Get_GetRegistryEnvironmentContainer()
        {
            // Generated from example definition: specification/machinelearningservices/resource-manager/Microsoft.MachineLearningServices/stable/2024-04-01/examples/Registry/EnvironmentContainer/get.json
            // this example is just showing the usage of "RegistryEnvironmentContainers_Get" operation, for the dependent resources, they will have to be created separately.

            // get your azure access token, for more details of how Azure SDK get your access token, please refer to https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line
            TokenCredential cred = new DefaultAzureCredential();
            // authenticate your client
            ArmClient client = new ArmClient(cred);

            // this example assumes you already have this MachineLearningRegistryEnvironmentContainerResource created on azure
            // for more information of creating MachineLearningRegistryEnvironmentContainerResource, please refer to the document of MachineLearningRegistryEnvironmentContainerResource
            string subscriptionId = "00000000-1111-2222-3333-444444444444";
            string resourceGroupName = "testrg123";
            string registryName = "testregistry";
            string environmentName = "testEnvironment";
            ResourceIdentifier machineLearningRegistryEnvironmentContainerResourceId = MachineLearningRegistryEnvironmentContainerResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, registryName, environmentName);
            MachineLearningRegistryEnvironmentContainerResource machineLearningRegistryEnvironmentContainer = client.GetMachineLearningRegistryEnvironmentContainerResource(machineLearningRegistryEnvironmentContainerResourceId);

            // invoke the operation
            MachineLearningRegistryEnvironmentContainerResource result = await machineLearningRegistryEnvironmentContainer.GetAsync();

            // the variable result is a resource, you could call other operations on this instance as well
            // but just for demo, we get its data from this resource instance
            MachineLearningEnvironmentContainerData resourceData = result.Data;
            // for demo we just print out the id
            Console.WriteLine($"Succeeded on id: {resourceData.Id}");
        }

        [Test]
        [Ignore("Only validating compilation of examples")]
        public async Task Delete_DeleteRegistryEnvironmentContainer()
        {
            // Generated from example definition: specification/machinelearningservices/resource-manager/Microsoft.MachineLearningServices/stable/2024-04-01/examples/Registry/EnvironmentContainer/delete.json
            // this example is just showing the usage of "RegistryEnvironmentContainers_Delete" operation, for the dependent resources, they will have to be created separately.

            // get your azure access token, for more details of how Azure SDK get your access token, please refer to https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line
            TokenCredential cred = new DefaultAzureCredential();
            // authenticate your client
            ArmClient client = new ArmClient(cred);

            // this example assumes you already have this MachineLearningRegistryEnvironmentContainerResource created on azure
            // for more information of creating MachineLearningRegistryEnvironmentContainerResource, please refer to the document of MachineLearningRegistryEnvironmentContainerResource
            string subscriptionId = "00000000-1111-2222-3333-444444444444";
            string resourceGroupName = "testrg123";
            string registryName = "testregistry";
            string environmentName = "testContainer";
            ResourceIdentifier machineLearningRegistryEnvironmentContainerResourceId = MachineLearningRegistryEnvironmentContainerResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, registryName, environmentName);
            MachineLearningRegistryEnvironmentContainerResource machineLearningRegistryEnvironmentContainer = client.GetMachineLearningRegistryEnvironmentContainerResource(machineLearningRegistryEnvironmentContainerResourceId);

            // invoke the operation
            await machineLearningRegistryEnvironmentContainer.DeleteAsync(WaitUntil.Completed);

            Console.WriteLine("Succeeded");
        }

        [Test]
        [Ignore("Only validating compilation of examples")]
        public async Task Update_CreateOrUpdateRegistryEnvironmentContainer()
        {
            // Generated from example definition: specification/machinelearningservices/resource-manager/Microsoft.MachineLearningServices/stable/2024-04-01/examples/Registry/EnvironmentContainer/createOrUpdate.json
            // this example is just showing the usage of "RegistryEnvironmentContainers_CreateOrUpdate" operation, for the dependent resources, they will have to be created separately.

            // get your azure access token, for more details of how Azure SDK get your access token, please refer to https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line
            TokenCredential cred = new DefaultAzureCredential();
            // authenticate your client
            ArmClient client = new ArmClient(cred);

            // this example assumes you already have this MachineLearningRegistryEnvironmentContainerResource created on azure
            // for more information of creating MachineLearningRegistryEnvironmentContainerResource, please refer to the document of MachineLearningRegistryEnvironmentContainerResource
            string subscriptionId = "00000000-1111-2222-3333-444444444444";
            string resourceGroupName = "testrg123";
            string registryName = "testregistry";
            string environmentName = "testEnvironment";
            ResourceIdentifier machineLearningRegistryEnvironmentContainerResourceId = MachineLearningRegistryEnvironmentContainerResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, registryName, environmentName);
            MachineLearningRegistryEnvironmentContainerResource machineLearningRegistryEnvironmentContainer = client.GetMachineLearningRegistryEnvironmentContainerResource(machineLearningRegistryEnvironmentContainerResourceId);

            // invoke the operation
            MachineLearningEnvironmentContainerData data = new MachineLearningEnvironmentContainerData(new MachineLearningEnvironmentContainerProperties
            {
                Description = "string",
                Tags =
{
["additionalProp1"] = "string",
["additionalProp2"] = "string",
["additionalProp3"] = "string"
},
                Properties =
{
["additionalProp1"] = "string",
["additionalProp2"] = "string",
["additionalProp3"] = "string"
},
            });
            ArmOperation<MachineLearningRegistryEnvironmentContainerResource> lro = await machineLearningRegistryEnvironmentContainer.UpdateAsync(WaitUntil.Completed, data);
            MachineLearningRegistryEnvironmentContainerResource result = lro.Value;

            // the variable result is a resource, you could call other operations on this instance as well
            // but just for demo, we get its data from this resource instance
            MachineLearningEnvironmentContainerData resourceData = result.Data;
            // for demo we just print out the id
            Console.WriteLine($"Succeeded on id: {resourceData.Id}");
        }
    }
}
