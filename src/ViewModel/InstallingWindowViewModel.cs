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
        /// Installing component title.
        /// </summary>
        public string Component
        {
            get => this.component;
            set => this.SetProperty(ref this.component, value);
        }

        /// <summary>
        /// Current executing action
        /// </summary>
        public Func<Task> CurrentAction { get; set; }
    }
}