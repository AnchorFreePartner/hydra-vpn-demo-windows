namespace Hydra.Sdk.Wpf
{
    using System.Windows;

    using Hydra.Sdk.Vpn.IoC;

    public partial class App : Application
    {
        /// <summary>
        /// Application startup logic.
        /// </summary>
        /// <param name="e">Arguments of the <see cref="Application.Startup"/> event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }
    }
}