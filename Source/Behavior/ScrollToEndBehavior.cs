// <copyright file="ScrollToEndBehavior.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.Behavior
{
    using System.Windows;
    using System.Windows.Controls;

    /// <summary>
    /// Scroll to end behavior for TextBox.
    /// </summary>
    internal class ScrollToEndBehavior
    {
        /// <summary>
        /// Attached ScrollToEndProperty.
        /// </summary>
        public static readonly DependencyProperty AlwaysScrollToEndProperty =
            DependencyProperty.RegisterAttached(
                "AlwaysScrollToEnd",
                typeof(bool),
                typeof(ScrollToEndBehavior),
                new UIPropertyMetadata(false, OnAlwaysScrollToEndChanged));

        /// <summary>
        /// Gets <see cref="AlwaysScrollToEndProperty"/> value for supplied <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> to get the value from.</param>
        /// <returns>Value of <see cref="AlwaysScrollToEndProperty"/>.</returns>
        public static bool GetAlwaysScrollToEnd(DependencyObject dependencyObject)
        {
            return (bool)dependencyObject.GetValue(AlwaysScrollToEndProperty);
        }

        /// <summary>
        /// Sets <see cref="AlwaysScrollToEndProperty"/> value for supplied <see cref="DependencyObject"/>.
        /// </summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> to set the value.</param>
        /// <param name="value">New value.</param>
        public static void SetAlwaysScrollToEnd(DependencyObject dependencyObject, bool value)
        {
            dependencyObject.SetValue(AlwaysScrollToEndProperty, value);
        }

        /// <summary>
        /// Property changing logic.
        /// </summary>
        /// <param name="dependencyObject"><see cref="DependencyObject"/> which has attached property.</param>
        /// <param name="e">Property changed event arguments.</param>
        private static void OnAlwaysScrollToEndChanged(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs e)
        {
            // Get TextBox instance
            var textBox = dependencyObject as TextBox;

            // Get new property value
            var newValue = (bool)e.NewValue;

            // Check above values for sanity
            if (textBox == null || (bool)e.OldValue == newValue)
            {
                return;
            }

            // Create event handler which scrolls to end event sender
            TextChangedEventHandler handler = (sender, args) =>
                ((TextBox)sender).ScrollToEnd();

            // If AlwaysScrollToEnd is true - attach handler, otherwise detach handler
            if (newValue)
            {
                textBox.TextChanged += handler;
            }
            else
            {
                textBox.TextChanged -= handler;
            }
        }
    }
}