// <copyright file="InstallingWindowViewModel.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.ViewModel
{
    using System;
    using System.Threading.Tasks;
    using Prism.Mvvm;

    /// <summary>
    /// Installing window view model.
    /// </summary>
    public class InstallingWindowViewModel : BindableBase
    {
        /// <summary>
        /// Installing component title.
        /// </summary>
        private string component;

        /// <summary>
        /// Gets or sets installing component title.
        /// </summary>
        public string Component
        {
            get => this.component;
            set => this.SetProperty(ref this.component, value);
        }

        /// <summary>
        /// Gets or sets current executing action.
        /// </summary>
        public Func<Task> CurrentAction { get; set; }
    }
}