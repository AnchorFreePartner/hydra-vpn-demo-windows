using Hydra.Sdk.Common.Logger;

namespace Hydra.Sdk.Wpf.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.ServiceProcess;
    using System.Threading.Tasks;

    /// <summary>
    /// Driver helper methods.
    /// </summary>
    internal static class DriverHelper
    {
        /// <summary>
        /// Driver files directory.
        /// </summary>
        private const string DriverDirectory = "Driver";

        /// <summary>
        /// Installer executable name.
        /// </summary>
        private const string InstallerExecutable = "tapinstall.exe";

        /// <summary>
        /// Driver file name.
        /// </summary>
        private const string DriverFileName = "aftap0901.sys";

        /// <summary>
        /// Inf file name.
        /// </summary>
        private const string InfFileName = "AFTap.inf";

        /// <summary>
        /// Driver HWID.
        /// </summary>
        private const string Hwid = "aftap0901";

        private static string PlatformPath => Environment.Is64BitOperatingSystem
            ? "64bit"
            : "32bit";

        /// <summary>
        /// Checks whether the driver is installed.
        /// </summary>
        /// <returns>true if driver is installed, false otherwise.</returns>
        public static bool IsInstalled()
        {
            var driversDirectory = Path.Combine(Environment.SystemDirectory, "drivers");
            var driverPath = Path.Combine(driversDirectory, DriverFileName);
            return File.Exists(driverPath) && IsServiceInstalled();
        }

        public static bool IsServiceInstalled()
        {
            if (string.IsNullOrWhiteSpace(Hwid))
            {
                return false;
            }

            using (var controller = new ServiceController(Hwid))
            {
                try
                {
                    return controller.Status == ServiceControllerStatus.Running;
                }
                catch
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Performs driver installation.
        /// </summary>
        /// <returns>true if driver was installed successfully, false otherwise.</returns>
        public static async Task<bool> InstallDriver()
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = Path.Combine(DriverDirectory, PlatformPath, InstallerExecutable),
                        Arguments = $"install \"{Path.Combine(DriverDirectory, PlatformPath, InfFileName)}\" \"{Hwid}\"",
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
                    HydraLogger.Error("Could not install TAP driver: {0}", e);
                    return false;
                }
            });
        }
    }
}