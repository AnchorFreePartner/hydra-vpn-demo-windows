// <copyright file="Bootstrapper.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>
// <summary>This is the Widget class.</summary>

namespace Hydra.Sdk.Demo
{
    using System.Windows;
    using Hydra.Sdk.Demo.View;
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
        protected override void InitializeShell()
        {
            base.InitializeShell();
            Application.Current.MainWindow = (Window)this.Shell;
            if (Application.Current.MainWindow == null)
            {
                return;
            }

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
    }
}