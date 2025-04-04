// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;

namespace Azure.ResourceManager.Nginx.Models
{
    /// <summary> The NginxConfigurationProtectedFileResult. </summary>
    public partial class NginxConfigurationProtectedFileResult
    {
        /// <summary>
        /// Keeps track of any properties unknown to the library.
        /// <para>
        /// To assign an object to the value of this property use <see cref="BinaryData.FromObjectAsJson{T}(T, System.Text.Json.JsonSerializerOptions?)"/>.
        /// </para>
        /// <para>
        /// To assign an already formatted json string to this property use <see cref="BinaryData.FromString(string)"/>.
        /// </para>
        /// <para>
        /// Examples:
        /// <list type="bullet">
        /// <item>
        /// <term>BinaryData.FromObjectAsJson("foo")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("\"foo\"")</term>
        /// <description>Creates a payload of "foo".</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromObjectAsJson(new { key = "value" })</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// <item>
        /// <term>BinaryData.FromString("{\"key\": \"value\"}")</term>
        /// <description>Creates a payload of { "key": "value" }.</description>
        /// </item>
        /// </list>
        /// </para>
        /// </summary>
        private IDictionary<string, BinaryData> _serializedAdditionalRawData;

        /// <summary> Initializes a new instance of <see cref="NginxConfigurationProtectedFileResult"/>. </summary>
        internal NginxConfigurationProtectedFileResult()
        {
        }

        /// <summary> Initializes a new instance of <see cref="NginxConfigurationProtectedFileResult"/>. </summary>
        /// <param name="virtualPath"> The virtual path of the protected file. </param>
        /// <param name="contentHash"> The hash of the content of the file. This value is used to determine if the file has changed. </param>
        /// <param name="serializedAdditionalRawData"> Keeps track of any properties unknown to the library. </param>
        internal NginxConfigurationProtectedFileResult(string virtualPath, string contentHash, IDictionary<string, BinaryData> serializedAdditionalRawData)
        {
            VirtualPath = virtualPath;
            ContentHash = contentHash;
            _serializedAdditionalRawData = serializedAdditionalRawData;
        }

        /// <summary> The virtual path of the protected file. </summary>
        public string VirtualPath { get; }
        /// <summary> The hash of the content of the file. This value is used to determine if the file has changed. </summary>
        public string ContentHash { get; }
    }
}
