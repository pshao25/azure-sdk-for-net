// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;

namespace Azure.ResourceManager.Peering.Models
{
    /// <summary> The details of the event associated with a prefix. </summary>
    public partial class PeeringServicePrefixEvent
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

        /// <summary> Initializes a new instance of <see cref="PeeringServicePrefixEvent"/>. </summary>
        internal PeeringServicePrefixEvent()
        {
        }

        /// <summary> Initializes a new instance of <see cref="PeeringServicePrefixEvent"/>. </summary>
        /// <param name="eventTimestamp"> The timestamp of the event associated with a prefix. </param>
        /// <param name="eventType"> The type of the event associated with a prefix. </param>
        /// <param name="eventSummary"> The summary of the event associated with a prefix. </param>
        /// <param name="eventLevel"> The level of the event associated with a prefix. </param>
        /// <param name="eventDescription"> The description of the event associated with a prefix. </param>
        /// <param name="serializedAdditionalRawData"> Keeps track of any properties unknown to the library. </param>
        internal PeeringServicePrefixEvent(DateTimeOffset? eventTimestamp, string eventType, string eventSummary, string eventLevel, string eventDescription, IDictionary<string, BinaryData> serializedAdditionalRawData)
        {
            EventTimestamp = eventTimestamp;
            EventType = eventType;
            EventSummary = eventSummary;
            EventLevel = eventLevel;
            EventDescription = eventDescription;
            _serializedAdditionalRawData = serializedAdditionalRawData;
        }

        /// <summary> The timestamp of the event associated with a prefix. </summary>
        public DateTimeOffset? EventTimestamp { get; }
        /// <summary> The type of the event associated with a prefix. </summary>
        public string EventType { get; }
        /// <summary> The summary of the event associated with a prefix. </summary>
        public string EventSummary { get; }
        /// <summary> The level of the event associated with a prefix. </summary>
        public string EventLevel { get; }
        /// <summary> The description of the event associated with a prefix. </summary>
        public string EventDescription { get; }
    }
}
