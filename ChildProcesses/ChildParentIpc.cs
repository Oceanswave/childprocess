// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildParentIpc.cs" company="Maierhofer Software, Germany">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System.ServiceModel;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)]
    public class ChildParentIpc : IChildParentIpc
    {
        public ChildProcessManager Manager { get; internal set; }
        #region Public Methods

        public void ChildIpcInit(int processId)
        {
            
        }

        /// <summary>
        /// The child alive.
        /// </summary>
        /// <param name="processId">
        /// The process id. 
        /// </param>
        public void ChildAlive(int processId)
        {
            Manager.OnChildAlive(processId);
        }

        #endregion
    }
}