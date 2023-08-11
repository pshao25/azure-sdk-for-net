// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Xml.Linq;
using Azure.Core;
using Azure.Core.Json;

namespace Azure.Developer.LoadTesting.Models
{
    /// <summary>
    /// model RoundTripModel {
    ///   requiredString: string;
    ///   requiredNullableInt: int32 | null;
    ///   requiredNullableString: string | null;
    ///   nonRequiredNullableInt?: int32 | null;
    ///   nonRequiredNullableString?: string | null;
    /// }
    /// </summary>
    public partial class RoundTripModel
    {
        private MutableJsonElement _element;

        /// <summary>
        /// Initializes a new instance of the <see cref="RoundTripModel"/> class.
        /// </summary>
        public RoundTripModel(string requiredString, int? requiredNullableInt, string requiredNullableString)
        {
            Argument.AssertNotNull(requiredString, nameof(requiredString));

            BinaryData utf8Json;
            using (MemoryStream stream = new())
            {
                using Utf8JsonWriter writer = new Utf8JsonWriter(stream);
                writer.WriteStartObject();
                writer.WritePropertyName("requiredString");
                writer.WriteStringValue(requiredString);
                if (requiredNullableInt != null)
                {
                    writer.WritePropertyName("requiredNullableInt");
                    writer.WriteNumberValue(requiredNullableInt.Value);
                }
                else
                {
                    writer.WriteNull("requiredNullableInt");
                }
                if (requiredNullableString != null)
                {
                    writer.WritePropertyName("requiredNullableString");
                    writer.WriteStringValue(requiredNullableString);
                }
                else
                {
                    writer.WriteNull("requiredNullableString");
                }
                writer.WriteEndObject();
                writer.Flush();
                stream.Position = 0;
                utf8Json = BinaryData.FromStream(stream);
            }
            _element = MutableJsonDocument.Parse(utf8Json).RootElement;
        }

        internal RoundTripModel(MutableJsonElement element)
        {
            _element = element;
        }

        /// <summary> Required string, illustrating a reference type property. </summary>
        public string RequiredString
        {
            get
            {
                if (_element.TryGetProperty("requiredString", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return default;
            }
            set => _element.SetProperty("requiredString", value);
        }

        /// <summary> Required nullable int. </summary>
        public int? RequiredNullableInt
        {
            get
            {
                if (_element.TryGetProperty("requiredNullableInt", out MutableJsonElement value))
                {
                    return value.GetInt32();
                }
                return default;
            }
            set => _element.SetProperty("requiredNullableInt", value);
        }

        /// <summary> Required nullable string. </summary>
        public string RequiredNullableString
        {
            get
            {
                if (_element.TryGetProperty("requiredNullableString", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return default;
            }
            set => _element.SetProperty("requiredNullableString", value);
        }

        /// <summary> Optional nullable int. </summary>
        public int? NonRequiredNullableInt
        {
            get
            {
                if (_element.TryGetProperty("nonRequiredNullableInt", out MutableJsonElement value))
                {
                    return value.GetInt32();
                }
                return null;
            }
            set => _element.SetProperty("nonRequiredNullableInt", value);
        }

        /// <summary> Optional nullable string. </summary>
        public string NonRequiredNullableString
        {
            get
            {
                if (_element.TryGetProperty("nonRequiredNullableString", out MutableJsonElement value))
                {
                    return value.GetString();
                }
                return null;
            }
            set => _element.SetProperty("nonRequiredNullableString", value);
        }
    }
}
