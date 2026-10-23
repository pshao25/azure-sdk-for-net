// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text.Json;

namespace Azure.ResourceManager.Compute.Models
{
    /// <summary> This type is obsolete. Cloud Services (classic) are no longer supported for new customers. </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("CloudService operations are no longer supported.")]
    public partial class CloudServiceExtension : IJsonModel<CloudServiceExtension>, IPersistableModel<CloudServiceExtension>
    {
        /// <summary> Initializes a new instance of CloudServiceExtension. </summary>
        public CloudServiceExtension()
        {
        }

        /// <summary> The name. </summary>
        public virtual string Name { get; set; }

        /// <summary> The publisher. </summary>
        public virtual string Publisher { get; set; }

        /// <summary> The extension type. </summary>
        public virtual string CloudServiceExtensionPropertiesType { get; set; }

        /// <summary> The type handler version. </summary>
        public virtual string TypeHandlerVersion { get; set; }

        /// <summary> Whether to auto-upgrade minor version. </summary>
        public virtual bool? AutoUpgradeMinorVersion { get; set; }

        /// <summary> The settings. </summary>
        public virtual BinaryData Settings { get; set; }

        /// <summary> The protected settings. </summary>
        public virtual BinaryData ProtectedSettings { get; set; }

        /// <summary> The protected settings from key vault. </summary>
        public virtual CloudServiceVaultAndSecretReference ProtectedSettingsFromKeyVault { get; set; }

        /// <summary> The force update tag. </summary>
        public virtual string ForceUpdateTag { get; set; }

        /// <summary> The provisioning state. </summary>
        public virtual string ProvisioningState { get; }

        /// <summary> The roles applied to. </summary>
        public virtual IList<string> RolesAppliedTo { get; set; }

        /// <summary> Writes the JSON representation of the model to the provided writer. </summary>
        protected virtual void JsonModelWriteCore(Utf8JsonWriter writer, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        CloudServiceExtension IJsonModel<CloudServiceExtension>.Create(ref Utf8JsonReader reader, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        void IJsonModel<CloudServiceExtension>.Write(Utf8JsonWriter writer, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        CloudServiceExtension IPersistableModel<CloudServiceExtension>.Create(BinaryData data, ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");

        string IPersistableModel<CloudServiceExtension>.GetFormatFromOptions(ModelReaderWriterOptions options) => "J";

        BinaryData IPersistableModel<CloudServiceExtension>.Write(ModelReaderWriterOptions options)
            => throw new NotSupportedException("CloudService operations are no longer supported.");
    }
}
