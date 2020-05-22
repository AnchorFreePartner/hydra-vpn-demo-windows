// <copyright file="Shell.xaml.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.View
{
    using System.ComponentModel;
    using System.Windows;
    using Hydra.Sdk.Wpf.Helper;
    using Hydra.Sdk.Wpf.ViewModel;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Main window.
    /// </summary>
    public partial class Shell : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Shell"/> class.
        /// <see cref="Shell"/> default constructor.
        /// </summary>
        public Shell()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets shell view model (injected).
        /// </summary>
        [Dependency]
        public ShellViewModel ShellViewModel
        {
            get => this.DataContext as ShellViewModel;
            set => this.DataContext = value;
        }

        /// <summary>
        /// Actions to perform on main window closing.
        /// </summary>
        protected override async void OnClosing(CancelEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            // Do not close window now
            e.Cancel = true;

            // Logout from backend
            await LogoutHelper.Logout().ConfigureAwait(false);

            // Shutdown application
            Application.Current.Shutdown();
        }
    }
}