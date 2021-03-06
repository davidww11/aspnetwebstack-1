﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace System.Web.Http.OData.Batch
{
    /// <summary>
    /// Represents a ChangeSet request.
    /// </summary>
    public class ChangeSetRequestItem : ODataBatchRequestItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ChangeSetRequestItem"/> class.
        /// </summary>
        /// <param name="requests">The request messages in the ChangeSet.</param>
        public ChangeSetRequestItem(IEnumerable<HttpRequestMessage> requests)
        {
            if (requests == null)
            {
                throw Error.ArgumentNull("requests");
            }

            Requests = requests;
        }

        /// <summary>
        /// Gets the request messages in the ChangeSet.
        /// </summary>
        public IEnumerable<HttpRequestMessage> Requests { get; private set; }

        /// <summary>
        /// Sends the ChangeSet request.
        /// </summary>
        /// <param name="invoker">The invoker.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A <see cref="ChangeSetResponseItem"/>.</returns>
        public override async Task<ODataBatchResponseItem> SendRequestAsync(HttpMessageInvoker invoker, CancellationToken cancellationToken)
        {
            if (invoker == null)
            {
                throw Error.ArgumentNull("invoker");
            }

            cancellationToken.ThrowIfCancellationRequested();

            Dictionary<string, string> contentIdToLocationMapping = new Dictionary<string, string>();
            List<HttpResponseMessage> responses = new List<HttpResponseMessage>();
            foreach (HttpRequestMessage request in Requests)
            {
                responses.Add(await SendMessageAsync(invoker, request, cancellationToken, contentIdToLocationMapping));
            }

            return new ChangeSetResponseItem(responses);
        }

        /// <summary>
        /// Gets the resources registered for dispose on each request messages of the ChangeSet.
        /// </summary>
        /// <returns>A collection of resources registered for dispose.</returns>
        public override IEnumerable<IDisposable> GetResourcesForDisposal()
        {
            List<IDisposable> resources = new List<IDisposable>();
            foreach (HttpRequestMessage request in Requests)
            {
                if (request != null)
                {
                    resources.AddRange(request.GetResourcesForDisposal());
                }
            }
            return resources;
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (HttpRequestMessage request in Requests)
                {
                    if (request != null)
                    {
                        request.Dispose();
                    }
                }
            }
        }
    }
}