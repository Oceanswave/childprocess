// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IParentChildIpc.cs" company="Maierhofer Software, Germany">
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
    [ServiceContract]
    public interface IParentChildIpc
    {
        #region Public Methods

        /// <summary>
        /// The parent alive.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void ParentAlive();

        /// <summary>
        /// The shutdown.
        /// </summary>
        [OperationContract(IsOneWay = true)]
        void Shutdown();

        #endregion
    }
}