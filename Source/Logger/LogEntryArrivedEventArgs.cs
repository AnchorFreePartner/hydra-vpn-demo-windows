// <copyright file="LogEntryArrivedEventArgs.cs" company="AnchorFree Inc.">
// Copyright (c) AnchorFree Inc. All rights reserved.
// </copyright>

namespace Hydra.Sdk.Wpf.Logger
{
    using System;

    /// <summary>
    /// <see cref="EventLoggerListener.LogEntryArrived"/> event arguments.
    /// </summary>
    internal class LogEntryArrivedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LogEntryArrivedEventArgs"/> class.
        /// <see cref="LogEntryArrivedEventArgs"/> constructor.
        /// </summary>
        /// <param name="entry">Log entry message.</param>
        public LogEntryArrivedEventArgs(string entry)
        {
            this.Entry = entry;
        }

        /// <summary>
        /// Gets log entry message.
        /// </summary>
        public string Entry { get; }
    }
}