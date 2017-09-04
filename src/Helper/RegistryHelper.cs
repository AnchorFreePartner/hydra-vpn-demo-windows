namespace Hydra.Sdk.Wpf.Helper
{
    using System;

    using Microsoft.Win32;

    /// <summary>
    /// Windows registry helper methods.
    /// </summary>
    internal static class RegistryHelper
    {
        /// <summary>
        /// Gets machine GUID from HKLM\SOFTWARE\Microsoft\Cryptography.
        /// </summary>
        /// <returns>Machine GUID.</returns>
        internal static string GetMachineGuid()
        {
            var localKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, Environment.Is64BitOperatingSystem ? RegistryView.Registry64 : RegistryView.Registry32);
            var openSubKey = localKey.OpenSubKey("SOFTWARE\\Microsoft\\Cryptography");
            return (string)openSubKey.GetValue("MachineGuid");
        }
    }
}