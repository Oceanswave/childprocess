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

    /// <summary>
    ///     The process instance.
    /// </summary>
    public class ProcessInstance
    {
        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ProcessInstance" /> class.
        /// </summary>
        protected ProcessInstance()
        {
            this.CurrentProcess = Process.GetCurrentProcess();
            this.WatchdogTimeout = new TimeSpan(0, 1, 0);
            this.AliveMessageFrquency = new TimeSpan(0, 0, 10);
            this.LastAliveMessage = DateTime.MinValue;
        }

        #endregion

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