// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChildParentIpc.cs" company="Maierhofer Software, Germany">
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
    [ServiceContract(CallbackContract = typeof(IParentChildIpc))]
    public interface IChildParentIpc
    {
        #region Public Methods

        [OperationContract]
        void ChildIpcInit(int processId);

        /// <summary>
        /// The child alive.
        /// </summary>
        /// <param name="processId">
        /// The process id. 
        /// </param>
        [OperationContract(IsOneWay = true)]
        void ChildAlive(int processId);

        #endregion
    }
}