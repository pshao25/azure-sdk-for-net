// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable
using System.IO;
using System.Text.Json;
using System;
using Azure.Core.Json;
using Azure.Core;
using System.Collections.Generic;

namespace Azure.Developer.LoadTesting.Models
{
    /// <summary>
    ///  model TestV2 {
    ///    @key
    ///    key: string; // Because it has @key so there will be a @path implictly added to it.
    ///
    ///    @visibility("read")
    ///    readOnlyProperty: string; // Add this propery to differentiate request from response.
    ///    optionalProperty?: string;
    ///    requiredProperty: int32;
    ///  }
    /// </summary>
    public partial class TestV2
    {
        private MutableJsonElement _element;

        /// <summary> </summary>
        public TestV2(string key, int required)
        {
            BinaryData utf8Json;
            using (MemoryStream stream = new())
            {
                using Utf8JsonWriter writer = new Utf8JsonWriter(stream);
                writer.WriteStartObject();
                writer.WritePropertyName("key");
                writer.WriteStringValue(key.AsSpan());
                writer.WritePropertyName("requiredProperty");
                writer.WriteNumberValue(required);
                writer.WriteEndObject();
                writer.Flush();
                stream.Position = 0;
                utf8Json = BinaryData.FromStream(stream);
            }
            _element = MutableJsonDocument.Parse(utf8Json).RootElement;
        }

        internal TestV2(MutableJsonElement element)
        {
            _element = element;
        }

        /// <summary></summary>
        public string Key
        {
            get
            {
                if (_element.TryGetProperty("key", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return default;
            }
            set => _element.SetProperty("key", value);
        }

        /// <summary> </summary>
        public int RequiredProperty
        {
            get
            {
                if (_element.TryGetProperty("requiredProperty", out MutableJsonElement value))
                {
                    return value.GetInt32();
                }
                return default;
            }
            set => _element.SetProperty("requiredProperty", value);
        }

        /// <summary> </summary>
        public string OptionalProperty
        {
            get
            {
                if (_element.TryGetProperty("optionalProperty", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return default;
            }
            set => _element.SetProperty("optionalProperty", value);
        }

        /// <summary> </summary>
        public string ReadOnlyProperty
        {
            get
            {
                if (_element.TryGetProperty("readOnlyProperty", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return default;
            }
        }
    }
}
