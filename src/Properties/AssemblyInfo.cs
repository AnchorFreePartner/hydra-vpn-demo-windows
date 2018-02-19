using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;

#if DEBUG
[assembly: AssemblyConfiguration("Debug")]
#else
[assembly: AssemblyConfiguration("Release")]
#endif

[assembly: AssemblyCompany("AnchorFree Inc.")]
[assembly: AssemblyProduct("Hydra Windows SDK")]
[assembly: AssemblyCopyright("Copyright © 2018 AnchorFree Inc.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

[assembly: ComVisible(false)]

[assembly: AssemblyVersion("1.0.2.108")]
[assembly: AssemblyFileVersion("1.0.2.108")]

[assembly: AssemblyTitle("Hydra.Sdk.Wpf")]
[assembly: AssemblyDescription("Hydra SDK WPF demo application")]

[assembly: ThemeInfo(ResourceDictionaryLocation.None, ResourceDictionaryLocation.SourceAssembly)]
