// <copyright file="RequestAuthCodeViewModel.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.ViewModel
{
    using Prism.Mvvm;

    /// <summary>
    /// Request authentication code window view model.
    /// </summary>
    public class RequestAuthCodeViewModel : BindableBase
    {
        /// <summary>
        /// Authentication code.
        /// </summary>
        private string authCode;

        /// <summary>
        /// Gets or sets authentication code.
        /// </summary>
        public string AuthCode
        {
            get => this.authCode;
            set
            {
                this.SetProperty(ref this.authCode, value);
                this.RaisePropertyChanged(nameof(this.IsOkButtonEnabled));
            }
        }

        /// <summary>
        /// Gets a value indicating whether oK button enabled flag.
        /// </summary>
        public bool IsOkButtonEnabled
        {
            get => !string.IsNullOrWhiteSpace(this.AuthCode);
        }
    }
}