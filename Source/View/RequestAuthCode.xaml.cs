namespace Hydra.Sdk.Wpf.View
{    
    using System.Windows;
    using Microsoft.Practices.Unity;
    using ViewModel;

    /// <summary>
    ///     Request authentication code window.
    /// </summary>
    public partial class RequestAuthCode : Window
    {
        /// <summary>
        /// <see cref="RequestAuthCode"/> default constructor.
        /// </summary>
        public RequestAuthCode()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Request authentication code view model (injected).
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