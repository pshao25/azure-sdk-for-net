// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;
using Azure.Core.Expressions.DataFactory;

namespace Azure.ResourceManager.DataFactory.Models
{
    /// <summary> A copy activity Dynamics CRM sink. </summary>
    public partial class DynamicsCrmSink : CopySink
    {
        /// <summary> Initializes a new instance of <see cref="DynamicsCrmSink"/>. </summary>
        /// <param name="writeBehavior"> The write behavior for the operation. </param>
        public DynamicsCrmSink(DynamicsSinkWriteBehavior writeBehavior)
        {
            WriteBehavior = writeBehavior;
            CopySinkType = "DynamicsCrmSink";
        }

        /// <summary> Initializes a new instance of <see cref="DynamicsCrmSink"/>. </summary>
        /// <param name="copySinkType"> Copy sink type. </param>
        /// <param name="writeBatchSize"> Write batch size. Type: integer (or Expression with resultType integer), minimum: 0. </param>
        /// <param name="writeBatchTimeout"> Write batch timeout. Type: string (or Expression with resultType string), pattern: ((\d+)\.)?(\d\d):(60|([0-5][0-9])):(60|([0-5][0-9])). </param>
        /// <param name="sinkRetryCount"> Sink retry count. Type: integer (or Expression with resultType integer). </param>
        /// <param name="sinkRetryWait"> Sink retry wait. Type: string (or Expression with resultType string), pattern: ((\d+)\.)?(\d\d):(60|([0-5][0-9])):(60|([0-5][0-9])). </param>
        /// <param name="maxConcurrentConnections"> The maximum concurrent connection count for the sink data store. Type: integer (or Expression with resultType integer). </param>
        /// <param name="disableMetricsCollection"> If true, disable data store metrics collection. Default is false. Type: boolean (or Expression with resultType boolean). </param>
        /// <param name="additionalProperties"> Additional Properties. </param>
        /// <param name="writeBehavior"> The write behavior for the operation. </param>
        /// <param name="ignoreNullValues"> The flag indicating whether to ignore null values from input dataset (except key fields) during write operation. Default is false. Type: boolean (or Expression with resultType boolean). </param>
        /// <param name="alternateKeyName"> The logical name of the alternate key which will be used when upserting records. Type: string (or Expression with resultType string). </param>
        /// <param name="bypassBusinessLogicExecution"> Controls the bypass of Dataverse custom business logic. Type: string (or Expression with resultType string). Type: string (or Expression with resultType string). </param>
        /// <param name="bypassPowerAutomateFlows"> Controls the bypass of Power Automate flows. Default is false. Type: boolean (or Expression with resultType boolean). </param>
        internal DynamicsCrmSink(string copySinkType, DataFactoryElement<int> writeBatchSize, DataFactoryElement<string> writeBatchTimeout, DataFactoryElement<int> sinkRetryCount, DataFactoryElement<string> sinkRetryWait, DataFactoryElement<int> maxConcurrentConnections, DataFactoryElement<bool> disableMetricsCollection, IDictionary<string, BinaryData> additionalProperties, DynamicsSinkWriteBehavior writeBehavior, DataFactoryElement<bool> ignoreNullValues, DataFactoryElement<string> alternateKeyName, DataFactoryElement<string> bypassBusinessLogicExecution, DataFactoryElement<bool> bypassPowerAutomateFlows) : base(copySinkType, writeBatchSize, writeBatchTimeout, sinkRetryCount, sinkRetryWait, maxConcurrentConnections, disableMetricsCollection, additionalProperties)
        {
            WriteBehavior = writeBehavior;
            IgnoreNullValues = ignoreNullValues;
            AlternateKeyName = alternateKeyName;
            BypassBusinessLogicExecution = bypassBusinessLogicExecution;
            BypassPowerAutomateFlows = bypassPowerAutomateFlows;
            CopySinkType = copySinkType ?? "DynamicsCrmSink";
        }

        /// <summary> Initializes a new instance of <see cref="DynamicsCrmSink"/> for deserialization. </summary>
        internal DynamicsCrmSink()
        {
        }

        /// <summary> The write behavior for the operation. </summary>
        public DynamicsSinkWriteBehavior WriteBehavior { get; set; }
        /// <summary> The flag indicating whether to ignore null values from input dataset (except key fields) during write operation. Default is false. Type: boolean (or Expression with resultType boolean). </summary>
        public DataFactoryElement<bool> IgnoreNullValues { get; set; }
        /// <summary> The logical name of the alternate key which will be used when upserting records. Type: string (or Expression with resultType string). </summary>
        public DataFactoryElement<string> AlternateKeyName { get; set; }
        /// <summary> Controls the bypass of Dataverse custom business logic. Type: string (or Expression with resultType string). Type: string (or Expression with resultType string). </summary>
        public DataFactoryElement<string> BypassBusinessLogicExecution { get; set; }
        /// <summary> Controls the bypass of Power Automate flows. Default is false. Type: boolean (or Expression with resultType boolean). </summary>
        public DataFactoryElement<bool> BypassPowerAutomateFlows { get; set; }
    }
}
