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
    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class ChildParentIpc : IChildParentIpc
    {
        #region Public Methods

        /// <summary>
        /// The child alive.
        /// </summary>
        /// <param name="processId">
        /// The process id. 
        /// </param>
        public void ChildAlive(int processId)
        {
            ChildProcessManager.DoOnChildAlive(processId);
        }

        #endregion
    }
}