// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
using Azure.Core.Json;
using Azure.Developer.LoadTesting.Models;
using NUnit.Framework;

namespace Azure.Developer.LoadTesting.Tests
{
    public class TestV2ClientTest
    {
        [Test]
        public void TestV2UpdateSerializeTest()
        {
            // RequiredProperty could not set
            TestV2Update testV2 = new TestV2Update();
            testV2.Key = "key";

            var payload = Deserialize(testV2.ToRequestContent());
            Assert.AreEqual(true, GetProperty(payload, "key", out var key));
            Assert.AreEqual("key", key);
            Assert.AreEqual(false, GetProperty(payload, "requiredProperty", out var requiredProperty));

            // RequiredProperty could be set null
            testV2 = new TestV2Update();
            testV2.RequiredProperty = null;
            payload = Deserialize(testV2.ToRequestContent());
            Assert.AreEqual(true, GetProperty(payload, "requiredProperty", out requiredProperty));
            Assert.AreEqual(null, requiredProperty);
        }

        [Test]
        // This test is make sure the behavior is the same as befpre
        public void TestV2SerializeTest()
        {
            // Should only provide public constructor with required properties
            TestV2 testV2 = new TestV2("key", 15);
            testV2.OptionalProperty = null; // Only optional property could set
            var payload = Deserialize(testV2.ToRequestContent());
            // When optional prperty is null, then it should be not in the payload
            Assert.AreEqual(false, GetProperty(payload, "optionalProperty", out var optionalProperty));
        }

        [Test]
        // This test is make sure the behavior is the same as befpre
        public void TestV2DeserializeTest()
        {
            // Required properties should be set
            var json1 = new
            {
                key = "key",
                requiredProperty = 15
            };

            MutableJsonDocument jsonDocument = MutableJsonDocument.Parse(new BinaryData(json1));
            TestV2 testV2 = new TestV2(jsonDocument.RootElement);
            Assert.AreEqual("key", testV2.Key);
            Assert.AreEqual(15, testV2.RequiredProperty);

            // If required property is not set, set to default value
            var json2 = new
            {
                key = "key"
            };
            jsonDocument = MutableJsonDocument.Parse(new BinaryData(json2));
            testV2 = new TestV2(jsonDocument.RootElement);
            Assert.AreEqual(0, testV2.RequiredProperty);

            // If required property is null. then throw error
            var json3 = "{\"key\": \"key\", \"requiredProperty\": null}";
            jsonDocument = MutableJsonDocument.Parse(json3);
            testV2 = new TestV2(jsonDocument.RootElement);
            // TO-DO: previously the error happens at deserialization after returning from service. Now it happens when we use that property.
            Assert.Throws<InvalidOperationException>(() => _ = testV2.RequiredProperty);
        }

        private static string Deserialize(RequestContent content)
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
