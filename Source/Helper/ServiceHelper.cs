// <copyright file="ServiceHelper.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.Helper
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Management;
    using System.Reflection;
    using System.Security.Cryptography;
    using System.ServiceProcess;
    using System.Threading.Tasks;
    using Hydra.Sdk.Windows.Logger;

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

                return await IsServiceUpToDate().ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Performs hydra service installation.
        /// </summary>
        /// <returns>true if istallation was successful, false otherwise.</returns>
        public static async Task<bool> InstallService()
        {
            return await RunServiceInstaller("install").ConfigureAwait(false);
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
                await UninstallService().ConfigureAwait(false);
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

            if (!string.Equals(installedServiceHash, currentServiceHash, StringComparison.Ordinal))
            {
                await UninstallService().ConfigureAwait(false);
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
#pragma warning disable CA5351 // Do Not Use Broken Cryptographic Algorithms
                    using (var md5 = MD5.Create())
#pragma warning restore CA5351 // Do Not Use Broken Cryptographic Algorithms
                    {
                        var hash = md5.ComputeHash(stream);
#pragma warning disable CA1308 // Normalize strings to uppercase
                        return BitConverter.ToString(hash).Replace("-", string.Empty).ToLowerInvariant();
#pragma warning restore CA1308 // Normalize strings to uppercase
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
            using (var searcher = new ManagementObjectSearcher(query))
            {
                var resultCollection = searcher.Get();

                foreach (var result in resultCollection)
                {
                    var resultPath = result.GetPropertyValue("PathName").ToString();
                    return resultPath.Substring(1, resultPath.Length - 2);  // Remove quotes
                }

                return null;
            }
        }

        /// <summary>
        /// Performs hydra service uninstallation.
        /// </summary>
        /// <returns>true if istallation was successful, false otherwise.</returns>
        private static async Task<bool> UninstallService()
        {
            return await RunServiceInstaller("uninstall").ConfigureAwait(false);
        }

        /// <summary>
        /// Runs hydra service installer.
        /// </summary>
        /// <param name="verb">One of the following: "install", "uninstall".</param>
        /// <returns>true if the process was successful.</returns>
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
                        RedirectStandardError = true,
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
            }).ConfigureAwait(false);
        }
    }
}