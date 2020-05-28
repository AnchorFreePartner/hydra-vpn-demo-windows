// <copyright file="RequestAuthCode.xaml.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Demo.View
{
    using System.Windows;
    using Hydra.Sdk.Demo.ViewModel;
    using Microsoft.Practices.Unity;

    /// <summary>
    ///     Request authentication code window.
    /// </summary>
    public partial class RequestAuthCode : Window
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestAuthCode"/> class.
        /// <see cref="RequestAuthCode"/> default constructor.
        /// </summary>
        public RequestAuthCode()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets request authentication code view model (injected).
        /// </summary>
        [Dependency]
        public RequestAuthCodeViewModel RequestAuthCodeViewModel
        {
            get => this.DataContext as RequestAuthCodeViewModel;
            set => this.DataContext = value;
        }

        private void OkButtonClicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }

        private void CancelButtonClicked(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }
    }
}