// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Text.Json;
using System.Xml.Linq;
using Azure.Core;
using Azure.Core.Json;

namespace Azure.Developer.LoadTesting.Models
{
    public partial class RoundTripModel : IUtf8JsonSerializable
    {
        void IUtf8JsonSerializable.Write(Utf8JsonWriter writer)
        {
            _element.WriteTo(writer, 'P'); // TO-DO: we need another option not calling 'P' but its logic is the same as 'P'
        }

        /// <summary> Deserializes the model from a raw response. </summary>
        /// <param name="response"> The response to deserialize the model from. </param>
        internal static RoundTripModel FromResponse(Response response)
        {
            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(response.Content);
            return new RoundTripModel(jsonDocument.RootElement);
        }

        /// <summary> Convert into a Utf8JsonRequestContent. </summary>
        internal RequestContent ToRequestContent()
        {
            var content = new Utf8JsonRequestContent();
            content.JsonWriter.WriteObjectValue(this);
            return content;
        }
    }
}
