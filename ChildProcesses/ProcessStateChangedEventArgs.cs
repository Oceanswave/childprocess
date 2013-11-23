// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessStateChangedEventArgs.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ProcessStateChangedEventArgs : EventArgs
    {
        #region Constructors and Destructors

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcessStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="childProcess">
        /// The child process.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        public ProcessStateChangedEventArgs(ChildProcess childProcess, ProcessStateChangedAction action, string data)
        {
            this.ChildProcess = childProcess;
            this.Action = action;
            this.Data = data;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets Action.
        /// </summary>
        public ProcessStateChangedAction Action { get; private set; }

        /// <summary>
        ///     Gets ChildProcess.
        /// </summary>
        public ChildProcess ChildProcess { get; private set; }

        /// <summary>
        ///     Gets Data.
        /// </summary>
        public string Data { get; private set; }

        #endregion
    }
}