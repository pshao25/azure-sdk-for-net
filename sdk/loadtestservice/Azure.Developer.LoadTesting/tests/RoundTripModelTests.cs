// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using Azure.Core;
using Azure.Developer.LoadTesting.Models;
using NUnit.Framework;
using NUnit.Framework.Internal;

namespace Azure.Developer.LoadTesting.Tests
{
    public class RoundTripModelTests
    {
        [Test]
        public void TestSolution1()
        {
            RoundTripModel model = new RoundTripModel("requiredString", null, null);
            model.RequiredNullableString = null;

            var payload = Serialize(model.ToRequestContent());
        }

        private static string Serialize(RequestContent content)
        {
            var memStream = new MemoryStream();
            content.WriteTo(memStream, default);
            memStream.Position = 0;
            var dsr = new StreamReader(memStream);
            return dsr.ReadToEnd();
        }

        private static bool GetProperty(string json, string name, out object value)
        {
            var document = JsonDocument.Parse(json);
            if (document.RootElement.TryGetProperty(name, out var element))
            {
                value = element.GetObject();
                return true;
            }
            value = null;
            return false;
        }
    }
}
