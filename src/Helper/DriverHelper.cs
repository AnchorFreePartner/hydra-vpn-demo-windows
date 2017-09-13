namespace Hydra.Sdk.Wpf.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
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
        private const string DriverFileName = "tap0901.sys";

        /// <summary>
        /// Inf file name.
        /// </summary>
        private const string InfFileName = "OemVista.inf";

        /// <summary>
        /// Driver HWID.
        /// </summary>
        private const string Hwid = "tap0901";

        /// <summary>
        /// Checks whether the driver is installed.
        /// </summary>
        /// <returns></returns>
        public static bool IsInstalled()
        {
            var driversDirectory = Path.Combine(Environment.SystemDirectory, "drivers");
            var driverPath = Path.Combine(driversDirectory, DriverFileName);
            return File.Exists(driverPath);
        }

        /// <summary>
        /// Performs driver installation.
        /// </summary>
        /// <returns>true if driver was installed successfully, false otherwise.</returns>
        public static async Task<bool> InstallDriver()
        {
            return await Task.Factory.StartNew(() =>
            {
                var processStartInfo = new ProcessStartInfo
                {
                    FileName = Path.Combine(DriverDirectory, InstallerExecutable),
                    Arguments = $"install \"{Path.Combine(DriverDirectory, InfFileName)}\" \"{Hwid}\"",
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
            });
        }
    }
}