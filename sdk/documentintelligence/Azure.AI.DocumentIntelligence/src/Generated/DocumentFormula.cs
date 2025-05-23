// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

// <auto-generated/>

#nullable disable

using System;
using System.Collections.Generic;

namespace Azure.AI.DocumentIntelligence
{
    /// <summary> A formula object. </summary>
    public partial class DocumentFormula
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

        /// <summary> Initializes a new instance of <see cref="DocumentFormula"/>. </summary>
        /// <param name="kind"> Formula kind. </param>
        /// <param name="value"> LaTex expression describing the formula. </param>
        /// <param name="span"> Location of the formula in the reading order concatenated content. </param>
        /// <param name="confidence"> Confidence of correctly extracting the formula. </param>
        /// <exception cref="ArgumentNullException"> <paramref name="value"/> is null. </exception>
        internal DocumentFormula(DocumentFormulaKind kind, string value, DocumentSpan span, float confidence)
        {
            Argument.AssertNotNull(value, nameof(value));

            Kind = kind;
            Value = value;
            Polygon = new ChangeTrackingList<float>();
            Span = span;
            Confidence = confidence;
        }

        /// <summary> Initializes a new instance of <see cref="DocumentFormula"/>. </summary>
        /// <param name="kind"> Formula kind. </param>
        /// <param name="value"> LaTex expression describing the formula. </param>
        /// <param name="polygon">
        /// Bounding polygon of the formula, with coordinates specified relative to the
        /// top-left of the page. The numbers represent the x, y values of the polygon
        /// vertices, clockwise from the left (-180 degrees inclusive) relative to the
        /// element orientation.
        /// </param>
        /// <param name="span"> Location of the formula in the reading order concatenated content. </param>
        /// <param name="confidence"> Confidence of correctly extracting the formula. </param>
        /// <param name="serializedAdditionalRawData"> Keeps track of any properties unknown to the library. </param>
        internal DocumentFormula(DocumentFormulaKind kind, string value, IReadOnlyList<float> polygon, DocumentSpan span, float confidence, IDictionary<string, BinaryData> serializedAdditionalRawData)
        {
            Kind = kind;
            Value = value;
            Polygon = polygon;
            Span = span;
            Confidence = confidence;
            _serializedAdditionalRawData = serializedAdditionalRawData;
        }

        /// <summary> Initializes a new instance of <see cref="DocumentFormula"/> for deserialization. </summary>
        internal DocumentFormula()
        {
        }

        /// <summary> Formula kind. </summary>
        public DocumentFormulaKind Kind { get; }
        /// <summary> LaTex expression describing the formula. </summary>
        public string Value { get; }
        /// <summary>
        /// Bounding polygon of the formula, with coordinates specified relative to the
        /// top-left of the page. The numbers represent the x, y values of the polygon
        /// vertices, clockwise from the left (-180 degrees inclusive) relative to the
        /// element orientation.
        /// </summary>
        public IReadOnlyList<float> Polygon { get; }
        /// <summary> Location of the formula in the reading order concatenated content. </summary>
        public DocumentSpan Span { get; }
        /// <summary> Confidence of correctly extracting the formula. </summary>
        public float Confidence { get; }
    }
}
