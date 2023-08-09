// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable

using Azure.Core.Serialization;
using Azure.Core;
using Azure.Core.Json;
using System.Text.Json;
using System;

namespace Azure.Developer.LoadTesting.Models
{
    public partial class TestV2 : IUtf8JsonSerializable, IJsonModelSerializable
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            writer.WriteStartObject();
            writer.WritePropertyName("key"u8);
            writer.WriteStringValue(Key);
            writer.WritePropertyName("requiredProperty"u8);
            writer.WriteNumberValue(RequiredProperty);
            if (Optional.IsDefined(OptionalProperty))
            {
                writer.WritePropertyName("optionalProperty"u8);
                writer.WriteStringValue(OptionalProperty);
            }
            writer.WriteEndObject();
        }

        void IJsonModelSerializable.Serialize(Utf8JsonWriter writer, ModelSerializerOptions options)
        {
            ((IUtf8JsonSerializable)this).Write(writer);
        }

        object IJsonModelSerializable.Deserialize(ref Utf8JsonReader reader, ModelSerializerOptions options)
        {
            JsonDocument doc = JsonDocument.ParseValue(ref reader);
            MutableJsonDocument mdoc = new MutableJsonDocument(doc, new JsonSerializerOptions());
            return new TestV2(mdoc.RootElement);
        }

        object IModelSerializable.Deserialize(BinaryData data, ModelSerializerOptions options)
        {
            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(data);
            return new TestV2(jsonDocument.RootElement);
        }

        BinaryData IModelSerializable.Serialize(ModelSerializerOptions options) => ModelSerializerHelper.SerializeToBinaryData(writer => ((IJsonModelSerializable)this).Serialize(writer, options));

        internal virtual RequestContent ToRequestContent()
        {
            var content = new Utf8JsonRequestContent();
            content.JsonWriter.WriteObjectValue(this);
            return content;
        }

        internal static TestV2 FromResponse(Response response)
        {
            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(response.Content);
            return new TestV2(jsonDocument.RootElement);
        }
    }
}
