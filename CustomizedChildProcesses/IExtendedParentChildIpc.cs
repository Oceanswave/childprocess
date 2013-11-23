// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IExtendedParentChildIpc.cs" company="">
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
    [ServiceContract]
    public interface IExtendedParentChildIpc : IParentChildIpc
    {
        #region Public Methods and Operators

        /// <summary>
        /// The send custom message to child.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        [OperationContract]
        void SendCustomMessageToChild(string message);

        #endregion
    }
}