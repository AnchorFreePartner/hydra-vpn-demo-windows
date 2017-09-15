using System;
using Hydra.Sdk.Common.Logger;

namespace Hydra.Sdk.Wpf.Helper
{
    using System.Diagnostics;
    using System.ServiceProcess;
    using System.Threading.Tasks;

    /// <summary>
    /// Service helper methods.
    /// </summary>
    internal static class ServiceHelper
    {
        /// <summary>
        /// Hydra service name.
        /// </summary>
        private const string ServiceName = "hydrasvc";

        /// <summary>
        /// Checks whether the hydra service is installed.
        /// </summary>
        /// <returns>true if hydra service is installed, false otherwise.</returns>
        public static bool IsInstalled()
        {
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                return false;
            }

            using (var controller = new ServiceController(ServiceName))
            {
                try
                {
                    var controllerStatus = controller.Status;
                }
                catch
                {
                    return false;
                }
                return true;
            }
        }

        /// <summary>
        /// Performs hydra service installation.
        /// </summary>
        /// <returns>true if istallation was successful, false otherwise.</returns>
        public static async Task<bool> InstallService()
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = "Hydra.Sdk.Windows.Service.exe",
                        Arguments = $"-install {ServiceName}",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        Verb = "runas",
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true
                    };

                    var installerProcess = Process.Start(processStartInfo);
                    installerProcess?.WaitForExit();

                    return installerProcess?.ExitCode == 0;
                }
                catch (Exception e)
                {
                    HydraLogger.Error("Could not install hydra service: {0}", e);
                    return false;
                }
            });
        }
    }
}