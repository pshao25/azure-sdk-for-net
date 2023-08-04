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
            if (Optional.IsDefined(Key)) // Required property, we could use null to know if it is set
            {
                writer.WritePropertyName("key"u8);
                writer.WriteStringValue(Key);
            }
            if (Optional.IsDefined(RequiredProperty))
            {
                writer.WritePropertyName("requiredProperty"u8);
                writer.WriteNumberValue(RequiredProperty.Value);
            }
            if (IsOptionalPropertySet()) // We could integrate this into Optional.IsDefined
            {
                if (OptionalProperty == null)
                {
                    writer.WriteNull("optionalProperty");
                }
                else
                {
                    writer.WritePropertyName("optionalProperty"u8);
                    writer.WriteStringValue(OptionalProperty);
                }
            }
            writer.WriteEndObject();
        }

        // All the below is copied from your code, you could skip it
        void IJsonModelSerializable.Serialize(Utf8JsonWriter writer, ModelSerializerOptions options)
        {
            // TODO: it would be nice to standardize on the type of Format.
            if (options.Format == "P")
            {
                _element.WriteTo(writer, 'P');
                return;
            }

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
            // TODO: Use options?

            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(data);
            return new TestV2(jsonDocument.RootElement);
        }

        BinaryData IModelSerializable.Serialize(ModelSerializerOptions options) => ModelSerializerHelper.SerializeToBinaryData(writer => ((IJsonModelSerializable)this).Serialize(writer, options));

        /// <summary>
        /// </summary>
        /// <param name="response"></param>
        public static explicit operator TestV2(Response response)
        {
            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(response.Content);
            return new TestV2(jsonDocument.RootElement);
        }

        /// <summary>
        /// </summary>
        /// <param name="test"></param>
        public static implicit operator RequestContent(TestV2 test)
        {
            return new Utf8JsonDelayedRequestContent(test);
        }
    }
}
