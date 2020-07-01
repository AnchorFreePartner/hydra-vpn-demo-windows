// <copyright file="MainScreen.xaml.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Demo.Control
{
    using System.Windows.Controls;
    using Hydra.Sdk.Demo.ViewModel.Control;
    using Microsoft.Practices.Unity;

    /// <summary>
    /// Main screen control.
    /// </summary>
    public partial class MainScreen : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MainScreen"/> class.
        /// <see cref="MainScreen"/> default constructor.
        /// </summary>
        public MainScreen()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Gets or sets main screen view model (<see cref="MainScreenViewModel"/>, injected).
        /// </summary>
        [Dependency]
        public MainScreenViewModel MainScreenViewModel
        {
            get => this.DataContext as MainScreenViewModel;
            set => this.DataContext = value;
        }
    }
}