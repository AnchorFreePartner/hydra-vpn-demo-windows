// <copyright file="App.xaml.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Demo
{
    using System;
    using System.Windows;

    public partial class App : Application
    {
        /// <summary>
        /// Application startup logic.
        /// </summary>
        /// <param name="e">Arguments of the <see cref="Application.Startup"/> event.</param>
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            AppDomain.CurrentDomain.UnhandledException += this.UnhandledExceptionHandler;

            var bootstrapper = new Bootstrapper();
            bootstrapper.Run();
        }

        private void UnhandledExceptionHandler(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                var exceptionType = ex.GetType();
                MessageBox.Show(ex.ToString(), "Unhandled exception", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }
    }
}