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
        private readonly HttpPipeline _pipeline = null;
        internal ClientDiagnostics ClientDiagnostics { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TestV2Client"/> class.
        /// </summary>
        public TestV2Client() { }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> PatchAsync(string key, TestV2Update resource, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNull(key, nameof(key));
            Argument.AssertNotNull(resource, nameof(resource));

            RequestContext context = FromCancellationToken(cancellationToken);
            Response response = await PatchAsync(key, resource.ToRequestContent(), context).ConfigureAwait(false);

            return Response.FromValue(TestV2.FromResponse(response), response);
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

        internal HttpMessage CreatePatchRequest(string key, RequestContent content, RequestContext context)
        {
            return new HttpMessage(null, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="resource"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> PutAsync(string key, TestV2 resource, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));
            Argument.AssertNotNull(resource, nameof(resource));

            RequestContext context = FromCancellationToken(cancellationToken);
            Response response = await PutAsync(key, resource.ToRequestContent(), context).ConfigureAwait(false);
            return Response.FromValue(TestV2.FromResponse(response), response);
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

        internal HttpMessage CreatePutRequest(string key, RequestContent content, RequestContext context)
        {
            return new HttpMessage(null, null);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cancellationToken"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response<TestV2>> GetTestV2Async(string key, CancellationToken cancellationToken = default)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));

            RequestContext context = FromCancellationToken(cancellationToken);
            Response response = await GetTestV2Async(key, context).ConfigureAwait(false);
            return Response.FromValue(TestV2.FromResponse(response), response);
        }

        /// <summary>
        /// </summary>
        /// <param name="key"></param>
        /// <param name="context"></param>
        /// <returns>A <see cref="Task{TResult}"/> representing the result of the asynchronous operation.</returns>
        public virtual async Task<Response> GetTestV2Async(string key, RequestContext context)
        {
            Argument.AssertNotNullOrEmpty(key, nameof(key));

            using var scope = ClientDiagnostics.CreateScope("WidgetManagerClient.GetTestV2");
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

        internal HttpMessage CreateGetRequest(string key, RequestContext context)
        {
            return new HttpMessage(null, null);
        }

        private static RequestContext DefaultRequestContext = new RequestContext();
        internal static RequestContext FromCancellationToken(CancellationToken cancellationToken = default)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                return DefaultRequestContext;
            }

            return new RequestContext() { CancellationToken = cancellationToken };
        }
    }
}
