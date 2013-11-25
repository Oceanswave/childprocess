// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildParentIpc.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System.ServiceModel;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChildParentIpc : IChildParentIpc
    {
        #region Public Properties

        /// <summary>
        ///     Gets the manager.
        /// </summary>
        public ChildProcessManager Manager { get; internal set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The child alive.
        /// </summary>
        /// <param name="processId">
        /// The process id.
        /// </param>
        public void ChildAlive(int processId)
        {
            this.Manager.OnChildAlive(processId);
        }

        /// <summary>
        /// The child ipc init.
        /// </summary>
        /// <param name="processId">
        /// The process id.
        /// </param>
        public void ChildIpcInit(int processId)
        {
        }

        #endregion
    }
}