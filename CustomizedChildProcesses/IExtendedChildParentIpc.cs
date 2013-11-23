// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedChildParentIpc.cs" company="">
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
    ///     TODO: Update summary.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IExtendedParentChildIpc))]
    public interface IExtendedChildParentIpc : IChildParentIpc
    {
        #region Public Methods and Operators

        /// <summary>
        /// The send custom message to parent.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [OperationContract]
        void SendCustomMessageToParent(string message);

        #endregion
    }
}