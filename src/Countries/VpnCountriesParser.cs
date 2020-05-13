using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;
using Hydra.Sdk.Wpf.Model;
using Hydra.Sdk.Wpf.Properties;
using PartnerApi.Model.Nodes;

namespace Hydra.Sdk.Wpf.Countries
{
    public class VpnCountriesParser
    {
        private const int MaxCountryNameLength = 15;
        private static readonly ResourceManager CountriesResourceManager;

        static VpnCountriesParser()
        {
            CountriesResourceManager = new ResourceManager(typeof(Resources));
        }

        /// <inheritdoc />
        public string ToCountryName(VpnNode vpnNode)
        {
            var finalName = GetFullName(vpnNode);
            return finalName;
        }

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
        public VpnNodeModel ToVpnNodeModel(VpnNode vpnNode)
        {
            if (vpnNode == null)
            {
                throw new ArgumentNullException(nameof(vpnNode));
            }

            var fullName = GetFullName(vpnNode);
            var carrierReduceName = vpnNode.Carrier?.Name;
            return new VpnNodeModel()
            {
                ServerModel = vpnNode,
                DisplayName = fullName,
                FullName = fullName,
                IsToolTipVisible = fullName.Length > MaxCountryNameLength,
                CarrierDisplayName = carrierReduceName,
            };
        }

        private static string GetFullName(VpnNode vpnNode)
        {
            if (string.IsNullOrWhiteSpace(vpnNode.ServerRepresentation))
            {
                return string.Empty;
            }

            var isoName = CountriesResourceManager.GetString(vpnNode.ServerRepresentation.ToUpperInvariant());

            var finalName = string.IsNullOrEmpty(isoName) ? vpnNode.ServerRepresentation : isoName;

            return finalName;
        }
    }
}