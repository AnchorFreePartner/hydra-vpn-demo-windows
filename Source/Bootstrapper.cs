// <copyright file="Bootstrapper.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>
// <summary>This is the Widget class.</summary>

namespace Hydra.Sdk.Wpf
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Hydra.Sdk.Wpf.Helper;
    using Hydra.Sdk.Wpf.View;
    using Microsoft.Practices.Unity;
    using Prism.Modularity;
    using Prism.Unity;

    /// <summary>
    /// PRISM bootstrapper for the sample application.
    /// </summary>
    public class Bootstrapper : UnityBootstrapper
    {
        /// <inheritdoc/>
        protected override DependencyObject CreateShell()
        {
            return this.Container.Resolve<Shell>();
        }

        /// <summary>
        /// Initializes the shell.
        /// </summary>
        protected override async void InitializeShell()
        {
            base.InitializeShell();

            this.EnsureDriverInstalled();
            await this.EnsureServiceInstalled().ConfigureAwait(false);

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// Creates the <see cref="IModuleCatalog" /> used by Prism.
        /// </summary>
        /// <remarks>The base implementation returns a new ModuleCatalog.</remarks>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            var catalog = new ModuleCatalog();
            catalog.AddModule(typeof(PrismModule));
            return catalog;
        }

        private async Task EnsureServiceInstalled()
        {
            var isInstalled = await ServiceHelper.IsInstalled().ConfigureAwait(false);

            if (!isInstalled)
            {
                var installWindow = this.Container.Resolve<InstallingWindow>();
                installWindow.InstallingWindowViewModel.Component = "hydra service";
                installWindow.InstallingWindowViewModel.CurrentAction = async () =>
                {
                    var result = await ServiceHelper.InstallService().ConfigureAwait(false);
                    if (!result)
                    {
                        MessageBox.Show("Unable to install hydra service!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(1);
                    }
                };
                installWindow.ShowDialog();
            }
        }

        private void EnsureDriverInstalled()
        {
            if (!DriverHelper.IsInstalled())
            {
                var installWindow = this.Container.Resolve<InstallingWindow>();
                installWindow.InstallingWindowViewModel.Component = "tap driver";
                installWindow.InstallingWindowViewModel.CurrentAction = async () =>
                {
                    var result = await DriverHelper.InstallDriver().ConfigureAwait(false);
                    if (!result)
                    {
                        MessageBox.Show("Unable to install tap driver!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        Environment.Exit(1);
                    }
                };
                installWindow.ShowDialog();
            }
        }
    }
}