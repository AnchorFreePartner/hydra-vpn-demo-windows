// <copyright file="VpnNodeModel.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.Model
{
    using PartnerApi.Model.Nodes;

    /// <summary>
    ///  The VpnNodeModel model. It describes objects, which contain required for the vpn connection info.
    /// </summary>
    public class VpnNodeModel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VpnNodeModel"/> class.
        /// </summary>
        public VpnNodeModel()
        {
            this.DisplayName = string.Empty;
        }

        /// <summary>
        /// Gets or sets SDK model.
        /// </summary>
        public VpnNode ServerModel { get; set; }

        /// <summary>
        /// Gets or sets display name for a virtual location.
        /// </summary>
        public string DisplayName { get; set; }
    }
}