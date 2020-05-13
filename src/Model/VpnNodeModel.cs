using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using PartnerApi.Model.Nodes;

namespace Hydra.Sdk.Wpf.Model
{
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

        /// <summary>
        /// Gets or sets the full country name, before reducing.
        /// </summary>
        public string FullName { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether is tool tip visible. Is ToolTip visible for country.
        /// </summary>
        public bool IsToolTipVisible { get; set; }

        /// <summary>
        /// Gets or sets the country flag image.
        /// </summary>
        public string CountryImage { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether a value indicating the visibility mark of country flag image.
        /// </summary>
        public bool IsCountryImageVisible { get; set; }

        /// <summary>
        /// Gets the carrier id.
        /// </summary>
        public string CarrierId => this.ServerModel.Carrier.Id;

        /// <summary>
        /// Gets the carrier name.
        /// </summary>
        public string CarrierName => this.ServerModel.Carrier.Name;

        /// <summary>
        /// Gets or sets the carrier display name.
        /// </summary>
        public string CarrierDisplayName { get; set; }
    }
}
