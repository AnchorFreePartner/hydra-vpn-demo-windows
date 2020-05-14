namespace Hydra.Sdk.Wpf.View
{
    using System.Windows;
    using Microsoft.Practices.Unity;
    using ViewModel;

    /// <summary>
    /// Installing component window.
    /// </summary>
    public partial class InstallingWindow : Window
    {
        /// <summary>
        /// <see cref="InstallingWindow"/> default constructor.
        /// </summary>
        public InstallingWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Installing window view model (injected).
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
            await this.InstallingWindowViewModel.CurrentAction.Invoke();
            this.Close();
        }
    }
}
