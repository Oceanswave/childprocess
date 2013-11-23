// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IChildParentIpc.cs" company="Maierhofer Software, Germany">
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
    [ServiceContract(CallbackContract = typeof(IParentChildIpc))]
    public interface IChildParentIpc
    {
        #region Public Methods and Operators

        /// <summary>
        /// The child alive.
        /// </summary>
        /// <param name="processId">
        /// The process id.
        /// </param>
        [OperationContract(IsOneWay = true)]
        void ChildAlive(int processId);

        /// <summary>
        /// The child ipc init.
        /// </summary>
        /// <param name="processId">
        /// The process id.
        /// </param>
        [OperationContract]
        void ChildIpcInit(int processId);

        #endregion
    }
}