namespace Hydra.Sdk.Wpf
{
    using System;
    using System.Threading.Tasks;
    using System.Windows;
    using Helper;
    using Hydra.Sdk.Wpf.View;
    using Microsoft.Practices.Unity;
    using Prism.Modularity;
    using Prism.Unity;

    /// <summary>
    /// PRISM bootstrapper for the sample application
    /// </summary>
    public class Bootstrapper : UnityBootstrapper
    {
        private async Task EnsureServiceInstalled()
        {
            var isInstalled = await ServiceHelper.IsInstalled();

            if (!isInstalled)
            {
                var installWindow = this.Container.Resolve<InstallingWindow>();
                installWindow.InstallingWindowViewModel.Component = "hydra service";
                installWindow.InstallingWindowViewModel.CurrentAction = async () =>
                {
                    var result = await ServiceHelper.InstallService();
                    if (!result)
                    {
                        MessageBox.Show("Unable to install hydra service!", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
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
                    var result = await DriverHelper.InstallDriver();
                    if (!result)
                    {
                        MessageBox.Show("Unable to install tap driver!", "Error", MessageBoxButton.OK,
                            MessageBoxImage.Error);
                        Environment.Exit(1);
                    }
                };
                installWindow.ShowDialog();
            }
        }

        /// <summary>Creates the shell or main window of the application.</summary>
        /// <returns>The shell of the application.</returns>
        /// <remarks>
        /// If the returned instance is a <see cref="T:System.Windows.DependencyObject" />, the
        /// <see cref="T:Prism.Bootstrapper" /> will attach the default <see cref="T:Prism.Regions.IRegionManager" /> of
        /// the application in its <see cref="F:Prism.Regions.RegionManager.RegionManagerProperty" /> attached property
        /// in order to be able to add regions by using the <see cref="F:Prism.Regions.RegionManager.RegionNameProperty" />
        /// attached property from XAML.
        /// </remarks>
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

            EnsureDriverInstalled();
            await EnsureServiceInstalled();

            Application.Current.MainWindow = (Window)this.Shell;
            Application.Current.MainWindow.Show();
        }

        /// <summary>
        /// Creates the <see cref="T:Prism.Modularity.IModuleCatalog" /> used by Prism.
        /// </summary>
        /// <remarks>The base implementation returns a new ModuleCatalog.</remarks>
        protected override IModuleCatalog CreateModuleCatalog()
        {
            var catalog = new ModuleCatalog();
            catalog.AddModule(typeof(PrismModule));
            return catalog;
        }
    }
}