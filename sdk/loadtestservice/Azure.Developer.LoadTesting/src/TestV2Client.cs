// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core.Serialization;
using Azure.Developer.LoadTesting.Models;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using System.Threading;
using System;
using Azure.Core.Pipeline;
using Azure.Core;

namespace Azure.Developer.LoadTesting
{
    /// <summary>
    /// </summary>
    public partial class TestV2Client
    {
        private readonly HttpPipeline _pipeline;
        internal ClientDiagnostics ClientDiagnostics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestV2Client"/> class.
        /// </summary>
        public TestV2Client() { }

        /// <summary>
        /// </summary>
        /// <param name="testV2"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> PatchAsync(TestV2 testV2, CancellationToken cancellationToken = default)
        {
            if (testV2 is not IJsonModelSerializable serializable)
            {
                throw new InvalidCastException("model is not serializable");
            }

            using Stream stream = new MemoryStream();
            using (Utf8JsonWriter writer = new(stream))
            {
                serializable.Serialize(writer, new ModelSerializerOptions("P"));
            }

            stream.Position = 0;
            RequestContent content = RequestContent.Create(stream);

            // TODO: was there a good way to get RequestContext without creating it new?
            RequestContext context = new() { CancellationToken = cancellationToken };

            Response response = await PatchAsync(testV2.Key, content, context).ConfigureAwait(false);

            return Response.FromValue((TestV2)response, response);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response> PatchAsync(string key, RequestContent content, RequestContext context = null)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));
            Argument.AssertNotNull(content, nameof(content));

            using var scope = ClientDiagnostics.CreateScope("TestV2Client.Patch");
            scope.Start();
            try
            {
                using HttpMessage message = CreatePatchRequest(key, content, context);
                return await _pipeline.ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        internal HttpMessage CreatePatchRequest(string testId, RequestContent content, RequestContext context)
        {
            return new HttpMessage(null, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="testV2"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> PutAsync(TestV2 testV2, CancellationToken cancellationToken = default)
        {
            testV2.CheckValidPutBody();

            if (testV2 is not IJsonModelSerializable serializable)
            {
                throw new InvalidCastException("model is not serializable");
            }

            using Stream stream = new MemoryStream();
            using (Utf8JsonWriter writer = new(stream))
            {
                serializable.Serialize(writer, new ModelSerializerOptions("P"));
            }

            stream.Position = 0;
            RequestContent content = RequestContent.Create(stream);

            // TODO: was there a good way to get RequestContext without creating it new?
            RequestContext context = new() { CancellationToken = cancellationToken };

            Response response = await PutAsync(testV2.Key, content, context).ConfigureAwait(false);

            return Response.FromValue((TestV2)response, response);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="content"></param>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response> PutAsync(string key, RequestContent content, RequestContext context = null)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));
            Argument.AssertNotNull(content, nameof(content));

            using var scope = ClientDiagnostics.CreateScope("TestV2Client.Put");
            scope.Start();
            try
            {
                using HttpMessage message = CreatePutRequest(key, content, context);
                return await _pipeline.ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        internal HttpMessage CreatePutRequest(string testId, RequestContent content, RequestContext context)
        {
            return new HttpMessage(null, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> GetAsync(string key, CancellationToken cancellationToken = default)
        {
            RequestContext context = new() { CancellationToken = cancellationToken };
            Response response = await GetAsync(key, context).ConfigureAwait(false);
            return Response.FromValue((TestV2)response, response);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response> GetAsync(string key, RequestContext context)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));

            using var scope = ClientDiagnostics.CreateScope("TestV2Client.Get");
            scope.Start();
            try
            {
                using HttpMessage message = CreateGetRequest(key, context);
                return await _pipeline.ProcessMessageAsync(message, context).ConfigureAwait(false);
            }
            catch (Exception e)
            {
                scope.Failed(e);
                throw;
            }
        }

        internal HttpMessage CreateGetRequest(string testId, RequestContext context)
        {
            return new HttpMessage(null, null);
        }
    }
}
