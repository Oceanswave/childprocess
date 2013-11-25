// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ParentChildIpc.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ParentChildIpc : IParentChildIpc
    {
        #region Public Properties

        /// <summary>
        ///     Gets or sets ChildProcessInstance.
        /// </summary>
        public ChildProcessInstance ChildProcessInstance { get; internal set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        ///     The parent alive.
        /// </summary>
        public void ParentAlive()
        {
            this.ChildProcessInstance.OnParentAlive();
        }

        /// <summary>
        ///     The parent ipc init.
        /// </summary>
        public void ParentIpcInit()
        {
            this.ChildProcessInstance.OnParentIpcInit();
        }

        /// <summary>
        ///     The shutdown.
        /// </summary>
        public void Shutdown()
        {
            this.ChildProcessInstance.OnShutdown();
        }

        #endregion
    }
}