// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;

namespace Azure.Messaging.EventGrid.SystemEvents
{
    /// <summary> Event data for Microsoft.Devices.DeviceConnected event. </summary>
    public partial class IotHubDeviceConnectedEventData : DeviceConnectionStateEventProperties
    {
        /// <summary> Initializes a new instance of <see cref="IotHubDeviceConnectedEventData"/>. </summary>
        /// <param name="deviceId"> The unique identifier of the device. This case-sensitive string can be up to 128 characters long, and supports ASCII 7-bit alphanumeric characters plus the following special characters: - : . + % _ &amp;#35; * ? ! ( ) , = `@` ; $ '. </param>
        /// <param name="hubName"> Name of the IoT Hub where the device was created or deleted. </param>
        /// <param name="deviceConnectionStateEventInfo"> Information about the device connection state event. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="deviceId"/>, <paramref name="hubName"/> or <paramref name="deviceConnectionStateEventInfo"/> is null. </exception>
        internal IotHubDeviceConnectedEventData(string deviceId, string hubName, DeviceConnectionStateEventInfo deviceConnectionStateEventInfo) : base(deviceId, hubName, deviceConnectionStateEventInfo)
        {
            Argument.AssertNotNull(deviceId, nameof(deviceId));
            Argument.AssertNotNull(hubName, nameof(hubName));
            Argument.AssertNotNull(deviceConnectionStateEventInfo, nameof(deviceConnectionStateEventInfo));
        }

        /// <summary> Initializes a new instance of <see cref="IotHubDeviceConnectedEventData"/>. </summary>
        /// <param name="deviceId"> The unique identifier of the device. This case-sensitive string can be up to 128 characters long, and supports ASCII 7-bit alphanumeric characters plus the following special characters: - : . + % _ &amp;#35; * ? ! ( ) , = `@` ; $ '. </param>
        /// <param name="moduleId"> The unique identifier of the module. This case-sensitive string can be up to 128 characters long, and supports ASCII 7-bit alphanumeric characters plus the following special characters: - : . + % _ &amp;#35; * ? ! ( ) , = `@` ; $ '. </param>
        /// <param name="hubName"> Name of the IoT Hub where the device was created or deleted. </param>
        /// <param name="deviceConnectionStateEventInfo"> Information about the device connection state event. </param>
        /// <param name="serializedAdditionalRawData"> Keeps track of any properties unknown to the library. </param>
        internal IotHubDeviceConnectedEventData(string deviceId, string moduleId, string hubName, DeviceConnectionStateEventInfo deviceConnectionStateEventInfo, IDictionary<string, BinaryData> serializedAdditionalRawData) : base(deviceId, moduleId, hubName, deviceConnectionStateEventInfo, serializedAdditionalRawData)
        {
        }

        /// <summary> Initializes a new instance of <see cref="IotHubDeviceConnectedEventData"/> for deserialization. </summary>
        internal IotHubDeviceConnectedEventData()
        {
        }
    }
}
