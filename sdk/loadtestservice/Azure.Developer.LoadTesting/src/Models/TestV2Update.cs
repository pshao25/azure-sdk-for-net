// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using Azure.Core;
using Azure.Core.Json;

namespace Azure.Developer.LoadTesting.Models
{
    /// <summary>
    /// It is the patch model of TestV2, so it would be like
    ///  model TestV2 {
    ///    @key
    ///    key?: string; // Because it has @key so there will be a @path implictly added to it.
    ///
    ///    @visibility("read")
    ///    readOnlyProperty?: string; // Add this propery to differentiate request from response.
    ///    optionalProperty?: string;
    ///    requiredProperty?: int32;
    ///  }
    /// </summary>
    public partial class TestV2Update
    {
        private MutableJsonElement _element;

        /// <summary> </summary>
        public TestV2Update()
        {
            _element = MutableJsonDocument.Parse(MutableJsonDocument.EmptyJson).RootElement;
        }

        internal TestV2Update(MutableJsonElement element)
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
                return null;
            }
            set
            {
                _element.SetProperty("key", value);
            }
        }

        /// <summary></summary>
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
                _element.SetProperty("requiredProperty", value);
            }
        }

        /// <summary></summary>
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

        // We don't generate ReadOnlyProperty as it will not in the request
    }
}
