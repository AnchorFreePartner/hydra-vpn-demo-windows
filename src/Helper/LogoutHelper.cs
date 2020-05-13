using PartnerApi;
using PartnerApi.Parameters;

namespace Hydra.Sdk.Wpf.Helper
{
    using System;
    using System.Threading.Tasks;

    using Hydra.Sdk.Common.IoC;

    /// <summary>
    /// Logout related properties and methods.
    /// </summary>
    public static class LogoutHelper
    {
        /// <summary>
        /// Access token to perform logout with.
        /// </summary>
        public static string AccessToken { private get; set; }

        /// <summary>
        /// Performs logout from backend.
        /// </summary>
        public static async Task Logout()
        {
            try
            {
                // Resolve backend service
                var partnerBackendService = HydraIoc.Container.Resolve<IBackendService>();
                var logoutParams = new LogoutParams(AccessToken);
                // Logout from backend
                await partnerBackendService.LogoutAsync(logoutParams);
            }
            catch
            {
                // Ignored
            }
        }
    }
}