// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Azure.Core;
using Azure.ResourceManager.Synapse.Models;

namespace Azure.ResourceManager.Synapse
{
    internal class SynapseDataConnectionValidationListResultOperationSource : IOperationSource<SynapseDataConnectionValidationListResult>
    {
        SynapseDataConnectionValidationListResult IOperationSource<SynapseDataConnectionValidationListResult>.CreateResult(Response response, CancellationToken cancellationToken)
        {
            using var document = JsonDocument.Parse(response.ContentStream, ModelSerializationExtensions.JsonDocumentOptions);
            return SynapseDataConnectionValidationListResult.DeserializeSynapseDataConnectionValidationListResult(document.RootElement);
        }

        async ValueTask<SynapseDataConnectionValidationListResult> IOperationSource<SynapseDataConnectionValidationListResult>.CreateResultAsync(Response response, CancellationToken cancellationToken)
        {
            using var document = await JsonDocument.ParseAsync(response.ContentStream, ModelSerializationExtensions.JsonDocumentOptions, cancellationToken).ConfigureAwait(false);
            return SynapseDataConnectionValidationListResult.DeserializeSynapseDataConnectionValidationListResult(document.RootElement);
        }
    }
}
