// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Text.Json;
using Azure.Core;
using Azure.Core.Json;
using Azure.Core.Serialization;

namespace Azure.Developer.LoadTesting.Models
{
    public partial class TestV2Update : IUtf8JsonSerializable, IJsonModelSerializable
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            _element.WriteTo(writer, 'P');
        }

        void IJsonModelSerializable.Serialize(Utf8JsonWriter writer, ModelSerializerOptions options)
        {
            _element.WriteTo(writer, 'P');
        }

        BinaryData IModelSerializable.Serialize(ModelSerializerOptions options) => ModelSerializerHelper.SerializeToBinaryData(writer => ((IJsonModelSerializable)this).Serialize(writer, options));

        object IJsonModelSerializable.Deserialize(ref Utf8JsonReader reader, ModelSerializerOptions options)
        {
            JsonDocument doc = JsonDocument.ParseValue(ref reader);
            MutableJsonDocument mdoc = new MutableJsonDocument(doc, new JsonSerializerOptions());
            return new TestV2Update(mdoc.RootElement); // We don't know what it means when a property is null
        }

        object IModelSerializable.Deserialize(BinaryData data, ModelSerializerOptions options)
        {
            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(data);
            return new TestV2Update(jsonDocument.RootElement);
        }

        internal virtual RequestContent ToRequestContent()
        {
            var content = new Utf8JsonRequestContent();
            content.JsonWriter.WriteObjectValue(this);
            return content;
        }
    }
}
