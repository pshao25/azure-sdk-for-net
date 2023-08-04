// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading.Tasks;
using Azure.Developer.LoadTesting.Models;
using NUnit.Framework;

namespace Azure.Developer.LoadTesting.Tests
{
    public class TestV2ClientTest
    {
        [Test]
        public async Task PathTestAsync()
        {
            // RequiredProperty could not set
            TestV2 testV2 = new TestV2();
            testV2.Key = "key";

            // RequiredProperty will not be in the payload if not set. Expected: {"key": "key"}
            TestV2 result = await new TestV2Client().PatchAsync(testV2);

            // RequiredProperty cannot be set null
            testV2 = new TestV2();
            testV2.RequiredProperty = null; // Throw error

            // OptionalProperty can be set to null
            testV2 = new TestV2();
            testV2.OptionalProperty = null; // No error

            // OptionalProperty will be in the payload if set to null. Expected: {"optionalProperty": null}
            result = await new TestV2Client().PatchAsync(testV2);

            // OptionalProperty will not be in the payload if not set. Expected: {}
            testV2 = new TestV2();
            result = await new TestV2Client().PatchAsync(testV2);

            // Returned result should have RequiredProperty and Key. If the response payload doesn't have a RequiredProperty value, should set RequiredProperty to 0.
        }

        [Test]
        public async Task PutTestAsync()
        {
            // All the required properties should be set.
            TestV2 testV2 = new TestV2();
            testV2.Key = "key";
            TestV2 result = await new TestV2Client().PatchAsync(testV2); // Should throw error because RequiredProperty is not set.
        }
    }
}
