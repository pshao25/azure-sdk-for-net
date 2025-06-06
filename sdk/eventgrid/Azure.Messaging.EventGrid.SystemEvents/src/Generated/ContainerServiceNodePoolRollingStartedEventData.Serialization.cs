// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.ClientModel.Primitives;
using System.Collections.Generic;
using System.Text.Json;
using Azure.Core;

namespace Azure.Messaging.EventGrid.SystemEvents
{
    public partial class ContainerServiceNodePoolRollingStartedEventData : IUtf8JsonSerializable, IJsonModel<ContainerServiceNodePoolRollingStartedEventData>
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer) => ((IJsonModel<ContainerServiceNodePoolRollingStartedEventData>)this).Write(writer, ModelSerializationExtensions.WireOptions);

        void IJsonModel<ContainerServiceNodePoolRollingStartedEventData>.Write(Utf8JsonWriter writer, ModelReaderWriterOptions options)
        {
            writer.WriteStartObject();
            JsonModelWriteCore(writer, options);
            writer.WriteEndObject();
        }

        /// <param name="writer"> The JSON writer. </param>
        /// <param name="options"> The client options for reading and writing models. </param>
        protected override void JsonModelWriteCore(Utf8JsonWriter writer, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>)this).GetFormatFromOptions(options) : options.Format;
            if (format != "J")
            {
                throw new FormatException($"The model {nameof(ContainerServiceNodePoolRollingStartedEventData)} does not support writing '{format}' format.");
            }

            base.JsonModelWriteCore(writer, options);
        }

        ContainerServiceNodePoolRollingStartedEventData IJsonModel<ContainerServiceNodePoolRollingStartedEventData>.Create(ref Utf8JsonReader reader, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>)this).GetFormatFromOptions(options) : options.Format;
            if (format != "J")
            {
                throw new FormatException($"The model {nameof(ContainerServiceNodePoolRollingStartedEventData)} does not support reading '{format}' format.");
            }

            using JsonDocument document = JsonDocument.ParseValue(ref reader);
            return DeserializeContainerServiceNodePoolRollingStartedEventData(document.RootElement, options);
        }

        internal static ContainerServiceNodePoolRollingStartedEventData DeserializeContainerServiceNodePoolRollingStartedEventData(JsonElement element, ModelReaderWriterOptions options = null)
        {
            options ??= ModelSerializationExtensions.WireOptions;

            if (element.ValueKind == JsonValueKind.Null)
            {
                return null;
            }
            string nodePoolName = default;
            IDictionary<string, BinaryData> serializedAdditionalRawData = default;
            Dictionary<string, BinaryData> rawDataDictionary = new Dictionary<string, BinaryData>();
            foreach (var property in element.EnumerateObject())
            {
                if (property.NameEquals("nodePoolName"u8))
                {
                    nodePoolName = property.Value.GetString();
                    continue;
                }
                if (options.Format != "W")
                {
                    rawDataDictionary.Add(property.Name, BinaryData.FromString(property.Value.GetRawText()));
                }
            }
            serializedAdditionalRawData = rawDataDictionary;
            return new ContainerServiceNodePoolRollingStartedEventData(nodePoolName, serializedAdditionalRawData);
        }

        BinaryData IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>.Write(ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>)this).GetFormatFromOptions(options) : options.Format;

            switch (format)
            {
                case "J":
                    return ModelReaderWriter.Write(this, options);
                default:
                    throw new FormatException($"The model {nameof(ContainerServiceNodePoolRollingStartedEventData)} does not support writing '{options.Format}' format.");
            }
        }

        ContainerServiceNodePoolRollingStartedEventData IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>.Create(BinaryData data, ModelReaderWriterOptions options)
        {
            var format = options.Format == "W" ? ((IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>)this).GetFormatFromOptions(options) : options.Format;

            switch (format)
            {
                case "J":
                    {
                        using JsonDocument document = JsonDocument.Parse(data, ModelSerializationExtensions.JsonDocumentOptions);
                        return DeserializeContainerServiceNodePoolRollingStartedEventData(document.RootElement, options);
                    }
                default:
                    throw new FormatException($"The model {nameof(ContainerServiceNodePoolRollingStartedEventData)} does not support reading '{options.Format}' format.");
            }
        }

        string IPersistableModel<ContainerServiceNodePoolRollingStartedEventData>.GetFormatFromOptions(ModelReaderWriterOptions options) => "J";

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static new ContainerServiceNodePoolRollingStartedEventData FromResponse(Response response)
        {
            using var document = JsonDocument.Parse(response.Content, ModelSerializationExtensions.JsonDocumentOptions);
            return DeserializeContainerServiceNodePoolRollingStartedEventData(document.RootElement);
        }

        /// <summary> Convert into a <see cref="RequestContent"/>. </summary>
        internal override RequestContent ToRequestContent()
        {
            var content = new Utf8JsonRequestContent();
            content.JsonWriter.WriteObjectValue(this, ModelSerializationExtensions.WireOptions);
            return content;
        }
    }
}
