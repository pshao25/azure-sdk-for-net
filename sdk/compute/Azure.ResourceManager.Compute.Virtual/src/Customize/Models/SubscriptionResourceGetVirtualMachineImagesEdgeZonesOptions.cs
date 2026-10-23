// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using Azure.Core;

namespace Azure.ResourceManager.Compute.Models
{
    // Backward-compat option-bag type re-introduced for source compatibility with
    // the previous (AutoRest-based) public surface. See sibling
    // SubscriptionResourceGetVirtualMachineImagesOptions.cs for full justification.
    /// <summary> The options for the <c>GetVirtualMachineImagesEdgeZones</c> method. </summary>
    public partial class SubscriptionResourceGetVirtualMachineImagesEdgeZonesOptions
    {
        /// <summary> Initializes a new instance of <see cref="SubscriptionResourceGetVirtualMachineImagesEdgeZonesOptions"/>. </summary>
        /// <param name="location"> The name of a supported Azure region. </param>
        /// <param name="edgeZone"> The name of the edge zone. </param>
        /// <param name="publisherName"> A valid image publisher. </param>
        /// <param name="offer"> A valid image publisher offer. </param>
        /// <param name="skus"> A valid image SKU. </param>
        public SubscriptionResourceGetVirtualMachineImagesEdgeZonesOptions(AzureLocation location, string edgeZone, string publisherName, string offer, string skus)
        {
            Location = location;
            EdgeZone = edgeZone;
            PublisherName = publisherName;
            Offer = offer;
            Skus = skus;
        }

        /// <summary> The name of a supported Azure region. </summary>
        public virtual AzureLocation Location { get; }
        /// <summary> The name of the edge zone. </summary>
        public virtual string EdgeZone { get; }
        /// <summary> A valid image publisher. </summary>
        public virtual string PublisherName { get; }
        /// <summary> A valid image publisher offer. </summary>
        public virtual string Offer { get; }
        /// <summary> A valid image SKU. </summary>
        public virtual string Skus { get; }
        /// <summary> The expand expression to apply on the operation. </summary>
        public virtual string Expand { get; set; }
        /// <summary> An integer value specifying the number of images to return that matches supplied values. </summary>
        public virtual int? Top { get; set; }
        /// <summary> Specifies the order of the results returned. Formatted as an OData query. </summary>
        public virtual string Orderby { get; set; }
    }
}
