// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

#nullable disable
using System.IO;
using System.Text.Json;
using System;
using Azure.Core.Json;
using Azure.Core;

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

        /// <summary> Could be used in put operation. </summary>
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

        internal void CheckValidPutBody()
        {
            if (Key == null || RequiredProperty == null)
            {
                throw new ArgumentNullException("Put Body should set all the required properties");
            }
        }

        /// <summary>
        /// Could be used in patch operation. But put operation cannot use it. I'm thinking maybe we could have a
        /// public TestV2 CreateEmptyTestV2()
        /// just for patch usage.
        /// </summary>
        public TestV2()
        {
            _element = new MutableJsonElement();
        }

        internal TestV2(MutableJsonElement element)
        {
            _element = element;

            if (!_element.TryGetProperty("key", out MutableJsonElement _))
            {
                Key = default(string);
            }

            if (!_element.TryGetProperty("requiredProperty", out MutableJsonElement _))
            {
                RequiredProperty = default(int); // If we dont do this, then RequiredProperty will become null, which is different behavior from before.
            }

            if (!_element.TryGetProperty("optionalProperty", out MutableJsonElement _))
            {
                OptionalProperty = default(string);
            }

            if (!_element.TryGetProperty("readOnlyProperty", out MutableJsonElement _))
            {
                ReadOnlyProperty = default(string);
            }
        }

        /// <summary> Key is an initially required one
        /// 1. It cannot be set to null even in a patch.
        /// 2. It can be empty. Therefore null means not set.
        /// </summary>
        public string Key
        {
            get
            {
                if (_element.TryGetProperty("key", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("A required property cannot be set null");
                }

                _element.SetProperty("key", value);
            }
        }

        /// <summary> RequiredProperty is an initially required one, same with `Key`. </summary>
        public int? RequiredProperty
        {
            get
            {
                if (_element.TryGetProperty("requiredProperty", out MutableJsonElement value))
                {
                    return value.GetInt32();
                }
                return null;
            }
            set
            {
                if (value == null)
                {
                    throw new InvalidOperationException("A required property cannot be set null");
                }

                _element.SetProperty("requiredProperty", value);
            }
        }

        /// <summary> OptionalProperty is an initially optional one. </summary>
        public string OptionalProperty
        {
            get
            {
                if (_element.TryGetProperty("optionalProperty", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return null;
            }
            set => _element.SetProperty("optionalProperty", value);
        }

        // Why I add this? Because in the serialization process, we need to know the property is set to null or just not set.
        // I believe it is only needed to optional property.
        internal bool IsOptionalPropertySet()
        {
            if (_element.TryGetProperty("optionalProperty", out MutableJsonElement _))
            {
                return true;
            }
            return false;
        }

        /// <summary> ReadOnlyProperty is a read only one. So only in response. </summary>
        public string ReadOnlyProperty { get; private set; }
    }
}
