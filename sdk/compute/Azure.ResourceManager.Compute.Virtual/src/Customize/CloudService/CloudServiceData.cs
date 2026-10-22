// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;
using Azure.Core;
using Azure.ResourceManager.Compute.Models;
using Azure.ResourceManager.Models;

namespace Azure.ResourceManager.Compute
{
    /// <summary> This type is obsolete. Cloud Services (classic) are no longer supported for new customers. </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("CloudService operations are no longer supported.")]
    public partial class CloudServiceData : TrackedResourceData, IJsonModel<CloudServiceData>, IPersistableModel<CloudServiceData>
    {
        /// <summary> Initializes a new instance of CloudServiceData. </summary>
        /// <param name="location"> The location. </param>
        public CloudServiceData(AzureLocation location) : base(location)
        {
        }

        /// <summary> Initializes a new instance of CloudServiceData for deserialization. </summary>
        internal CloudServiceData()
        {
        }

        /// <summary> List of availability zones. </summary>
        public virtual IList<string> Zones { get; }

        /// <summary> The package URL. </summary>
        public virtual Uri PackageUri { get; set; }

        /// <summary> The XML service configuration (.cscfg). </summary>
        public virtual string Configuration { get; set; }

        /// <summary> The configuration URL. </summary>
        public virtual Uri ConfigurationUri { get; set; }

        /// <summary> Whether to start the cloud service. </summary>
        public virtual bool? StartCloudService { get; set; }

        /// <summary> Whether model overrides are allowed. </summary>
        public virtual bool? AllowModelOverride { get; set; }

        /// <summary> The upgrade mode. </summary>
        public virtual CloudServiceUpgradeMode? UpgradeMode { get; set; }

        /// <summary> The role profile properties. </summary>
        public virtual IList<CloudServiceRoleProfileProperties> Roles { get; }

        /// <summary> The OS secrets. </summary>
        public virtual IList<CloudServiceVaultSecretGroup> OSSecrets { get; }

        /// <summary> The network profile. </summary>
        public virtual CloudServiceNetworkProfile NetworkProfile { get; set; }

        /// <summary> The extensions. </summary>
        public virtual IList<CloudServiceExtension> Extensions { get; }

        /// <summary> The provisioning state. </summary>
        public virtual string ProvisioningState { get; }

        /// <summary> The unique identifier. </summary>
        public virtual string UniqueId { get; }

        /// <inheritdoc />
        protected override void JsonModelWriteCore(Utf8JsonWriter writer, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        CloudServiceData IJsonModel<CloudServiceData>.Create(ref Utf8JsonReader reader, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        void IJsonModel<CloudServiceData>.Write(Utf8JsonWriter writer, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        CloudServiceData IPersistableModel<CloudServiceData>.Create(BinaryData data, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        string IPersistableModel<CloudServiceData>.GetFormatFromOptions(ModelReaderWriterOptions options) => "J";

        BinaryData IPersistableModel<CloudServiceData>.Write(ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");
    }
}
