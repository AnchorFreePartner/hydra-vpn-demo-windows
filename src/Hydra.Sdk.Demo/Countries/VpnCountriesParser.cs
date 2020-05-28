// <copyright file="VpnCountriesParser.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Demo.Countries
{
    using System;
    using System.Globalization;
    using System.Resources;
    using Hydra.Sdk.Demo.Model;
    using Hydra.Sdk.Demo.Properties;
    using PartnerApi.Model.Nodes;

    /// <summary>
    /// The Vpn Countries Parser.
    /// </summary>
    public static class VpnCountriesParser
    {
        private static readonly ResourceManager CountriesResourceManager = new ResourceManager(typeof(Resources));

        /// <summary>
        /// The to vpn country.
        /// </summary>
        /// <param name="vpnNode">
        /// The server vpnNode model.
        /// </param>
        /// <returns>
        /// The <see cref="VpnNodeModel"/>.
        /// </returns>
        /// <exception cref="ArgumentNullException" >rises ArgumentNullException.</exception>
        public static VpnNodeModel ToVpnNodeModel(VpnNode vpnNode)
        {
            if (vpnNode == null)
            {
                throw new ArgumentNullException(nameof(vpnNode));
            }

            var fullName = GetFullName(vpnNode);
            return new VpnNodeModel
            {
                ServerModel = vpnNode,
                DisplayName = fullName,
            };
        }

        private static string GetFullName(VpnNode vpnNode)
        {
            if (string.IsNullOrWhiteSpace(vpnNode.ServerRepresentation))
            {
                return string.Empty;
            }

            var isoName = CountriesResourceManager.GetString(vpnNode.ServerRepresentation.ToUpperInvariant(), CultureInfo.InvariantCulture);

            var finalName = string.IsNullOrEmpty(isoName) ? vpnNode.ServerRepresentation : isoName;

            return finalName;
        }
    }
}