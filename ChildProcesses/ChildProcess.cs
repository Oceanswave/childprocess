// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildProcess.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   The Child Process represents the child process on the parent. It is manages by a <see cref="ChildProcessManager" />
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Diagnostics;

    /// <summary>
    ///     The Child Process represents the child process on the parent. It is manages by a <see cref="ChildProcessManager" />
    /// </summary>
    public class ChildProcess
    {
        #region Fields

        /// <summary>
        ///     The last time alive.
        /// </summary>
        internal DateTime lastTimeAlive;

        /// <summary>
        ///     The ipc channel avail.
        /// </summary>
        internal bool parentChildIpcChannelAvail;

        /// <summary>
        ///     The watchdog timeout.
        /// </summary>
        internal bool watchdogTimeout;

        /// <summary>
        ///     The parent child ipc.
        /// </summary>
        private IParentChildIpc parentChildIpc;

        /// <summary>
        ///     The parent child ipc lock.
        /// </summary>
        private object parentChildIpcLock = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChildProcess" /> class.
        /// </summary>
        public ChildProcess()
        {
            this.lastTimeAlive = DateTime.Now;
        }

        #endregion

        #region Public Properties

        /// <summary>
        ///     Gets or sets AdditionalData.
        /// </summary>
        public object AdditionalData { get; set; }

        /// <summary>
        ///     Gets or sets Manager.
        /// </summary>
        public ChildProcessManager Manager { get; internal set; }

        /// <summary>
        ///     Gets or sets Name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     Gets or sets ParentChildIpc.
        /// </summary>
        public IParentChildIpc ParentChildIpc
        {
            get
            {
                lock (this.parentChildIpcLock)
                {
                    return this.parentChildIpc;
                }
            }

            internal set
            {
                lock (this.parentChildIpcLock)
                {
                    this.parentChildIpc = value;
                }
            }
        }

        /// <summary>
        ///     Gets Process.
        /// </summary>
        public Process Process { get; private set; }

        #endregion

        #region Methods

        /// <summary>
        ///     The post start init.
        /// </summary>
        protected internal virtual void PostStartInit()
        {
            this.Process.BeginOutputReadLine();
            this.Process.BeginErrorReadLine();
        }

        /// <summary>
        /// The pre start init.
        /// </summary>
        /// <param name="manager">
        /// The manager.
        /// </param>
        /// <param name="process">
        /// The process.
        /// </param>
        protected internal virtual void PreStartInit(ChildProcessManager manager, Process process)
        {
            this.Manager = manager;
            this.Process = process;
            this.Process.OutputDataReceived += new DataReceivedEventHandler(this.Process_OutputDataReceived);
            this.Process.ErrorDataReceived += new DataReceivedEventHandler(this.Process_ErrorDataReceived);
        }

        /// <summary>
        /// The process_ error data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Process_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Manager.RaiseProcessStateChangedEvent(this, ProcessStateChangedEnum.StandardErrorMessage, e.Data);
        }

        /// <summary>
        /// The process_ output data received.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            this.Manager.RaiseProcessStateChangedEvent(this, ProcessStateChangedEnum.StandardOutputMessage, e.Data);
        }

        #endregion
    }
}