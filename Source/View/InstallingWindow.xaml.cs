// <copyright file="InstallingWindow.xaml.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.View
{
    using System.Windows;
    using Hydra.Sdk.Wpf.ViewModel;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Installing component window.
    /// </summary>
    public partial class InstallingWindow : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="InstallingWindow"/> class.
        /// <see cref="InstallingWindow"/> default constructor.
        /// </summary>
        public InstallingWindow()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets installing window view model (injected).
        /// </summary>
        [Dependency]
        public InstallingWindowViewModel InstallingWindowViewModel
        {
            get => this.DataContext as InstallingWindowViewModel;
            set => this.DataContext = value;
        }

        /// <summary>
        /// Installing window loaded event handler.
        /// </summary>
        private async void InstallingWindowOnLoaded(object sender, RoutedEventArgs e)
        {
            await this.InstallingWindowViewModel.CurrentAction.Invoke().ConfigureAwait(false);
            this.Close();
        }
    }
}