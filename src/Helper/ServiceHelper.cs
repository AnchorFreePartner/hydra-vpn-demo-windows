using System;
using Hydra.Sdk.Windows.Logger;

namespace Hydra.Sdk.Wpf.Helper
{
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Security.Cryptography;
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

        private const string HydraSdkWindowsServiceExecutable = "Hydra.Sdk.Windows.Service.exe";

        /// <summary>
        /// Checks whether the hydra service is installed.
        /// </summary>
        /// <returns>true if hydra service is installed, false otherwise.</returns>
        public static async Task<bool> IsInstalled()
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

                return await IsServiceUpToDate();
            }
        }

        /// <summary>
        /// Checks whether installed service is up to date. If it is not, removes it.
        /// </summary>
        /// <returns>true if service is up to date.</returns>
        private static async Task<bool> IsServiceUpToDate()
        {
            var servicePath = GetServicePath(ServiceName);
            if (string.IsNullOrWhiteSpace(servicePath))
            {
                await UninstallService();
                return false;
            }

            var currentDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            if (currentDirectory == null)
            {
                // We could not get current directory, so can't do further checks
                return true;
            }

            var currentServicePath = Path.Combine(currentDirectory, HydraSdkWindowsServiceExecutable);

            if (string.Equals(servicePath, currentServicePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var installedServiceHash = CalculateFileHash(servicePath);
            var currentServiceHash = CalculateFileHash(currentServicePath);

            if (!string.Equals(installedServiceHash, currentServiceHash))
            {
                await UninstallService();
                return false;
            }

            return true;
        }

        private static string CalculateFileHash(string path)
        {
            try
            {
                using (var stream = File.Open(path, FileMode.Open, FileAccess.Read))
                {
                    using (var md5 = MD5.Create())
                    {
                        var hash = md5.ComputeHash(stream);
                        return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant(); ;
                    }
                }
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }

        private static string GetServicePath(string path)
        {
            var query = new WqlObjectQuery($"SELECT * FROM Win32_Service WHERE Name = '{path}'");
            var searcher = new ManagementObjectSearcher(query);
            var resultCollection = searcher.Get();

            foreach (var result in resultCollection)
            {
                var resultPath = result.GetPropertyValue("PathName").ToString();
                return resultPath.Substring(1, resultPath.Length - 2);  // Remove quotes
            }

            return null;
        }

        /// <summary>
        /// Performs hydra service installation.
        /// </summary>
        /// <returns>true if istallation was successful, false otherwise.</returns>
        public static async Task<bool> InstallService()
        {
            return await RunServiceInstaller("install");
        }

        /// <summary>
        /// Performs hydra service uninstallation.
        /// </summary>
        /// <returns>true if istallation was successful, false otherwise.</returns>
        private static async Task<bool> UninstallService()
        {
            return await RunServiceInstaller("uninstall");
        }

        /// <summary>
        /// Runs hydra service installer.
        /// </summary>
        /// <param name="verb">One of the following: "install", "uninstall".</param>
        /// <returns>true if the process was successful</returns>
        private static async Task<bool> RunServiceInstaller(string verb)
        {
            return await Task.Factory.StartNew(() =>
            {
                try
                {
                    var processStartInfo = new ProcessStartInfo
                    {
                        FileName = HydraSdkWindowsServiceExecutable,
                        Arguments = $"-{verb} {ServiceName}",
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