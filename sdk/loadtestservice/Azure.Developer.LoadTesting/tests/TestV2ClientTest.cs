// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.Core;
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
