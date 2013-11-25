// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProcessInstance.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   The process instance.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Diagnostics;
    using System.Runtime.Remoting.Messaging;
    using System.Threading;

    /// <summary>
    ///     Process instance is the abstract instance of a running process - parent or child
    /// </summary>
    public abstract class ProcessInstance: IDisposable
    {

        private Thread watchdogThread;

        private AutoResetEvent watchdogEvent = new AutoResetEvent(false);

        private bool shutdown;

        /// <summary>
        /// Gets or sets the watchdog interval.
        /// </summary>
        /// <value>
        /// The watchdog interval.
        /// </value>
        TimeSpan WatchdogInterval { get; set; }

        public void TriggerWatchdog()
        {
            watchdogEvent.Set();
        }

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessInstance" /> class.
        /// </summary>
        protected ProcessInstance()
        {
            this.CurrentProcess = Process.GetCurrentProcess();
            WatchdogInterval = TimeSpan.FromMilliseconds(500);
            this.WatchdogTimeout = new TimeSpan(0, 1, 0);
            this.AliveMessageFrquency = new TimeSpan(0, 0, 10);
            this.LastAliveMessage = DateTime.MinValue;
        }

        #endregion

        /// <summary>
        /// Starts the watchdog.
        /// </summary>
        protected void StartWatchdog()
        {
            watchdogThread = new Thread(WatchdogThreadFkt);
            watchdogThread.Name = "ChildProcesses.Watchdog";
            watchdogThread.Priority = ThreadPriority.AboveNormal;
            watchdogThread.Start();
        }

        /// <summary>
        /// The watchdog thread function
        /// </summary>
        void WatchdogThreadFkt()
        {
            while (! shutdown)
            {
                watchdogEvent.WaitOne(WatchdogInterval);
                if (shutdown) return;
                this.OnWatchdog();
            }
        }

        /// <summary>
        /// Called when the watchdog interval is over
        /// </summary>
        protected abstract void OnWatchdog();

        /// <summary>
        /// Gets a value indicating whether is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }


        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            if (!this.IsDisposed)
            {
                this.IsDisposed = true;
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ChildProcessManager"/> class. 
        /// </summary>
        ~ProcessInstance()
        {
            this.IsDisposed = true;
            this.Dispose(false);
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            shutdown = true;
            if (watchdogThread != null)
            {
                watchdogEvent.Set();
                watchdogThread.Join(2000);
                if (watchdogThread.IsAlive)
                {
                    watchdogThread.Abort();
                    watchdogThread.Join(2000);
                }
                watchdogThread = null;
                watchdogEvent = null;
            }
        }


        #region Public Properties

        /// <summary>
        ///     Gets or sets AliveMessageFrquency.
        /// </summary>
        public TimeSpan AliveMessageFrquency { get; set; }

        /// <summary>
        ///     Gets CurrentProcess.
        /// </summary>
        public Process CurrentProcess { get; private set; }

        /// <summary>
        ///     Gets or sets WatchdogTimeout.
        /// </summary>
        public TimeSpan WatchdogTimeout { get; set; }

        #endregion

        #region Properties

        /// <summary>
        ///     Gets or sets LastAliveMessage.
        /// </summary>
        protected DateTime LastAliveMessage { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// The get ipc url prefix.
        /// </summary>
        /// <returns>
        /// The <see cref="string"/>.
        /// </returns>
        protected virtual string GetIpcUrlPrefix()
        {
            return "F67AC46D9F6C48AEBA42FD02236CCCDDXX";
        }

        #endregion
    }
}