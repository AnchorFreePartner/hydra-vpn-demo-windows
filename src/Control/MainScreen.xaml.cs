namespace Hydra.Sdk.Wpf.Control
{
    using System.Windows.Controls;

    using Hydra.Sdk.Wpf.ViewModel.Control;

    using Microsoft.Practices.Unity;

    /// <summary>
    /// Main screen control.
    /// </summary>
    public partial class MainScreen : UserControl
    {
        /// <summary>
        /// <see cref="MainScreen"/> default constructor.
        /// </summary>
        public MainScreen()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Main screen view model (<see cref="MainScreenViewModel"/>, injected).
        /// </summary>
        [Dependency]
        public MainScreenViewModel MainScreenViewModel
        {
            get => this.DataContext as MainScreenViewModel;
            set => this.DataContext = value;
        }
    }
}