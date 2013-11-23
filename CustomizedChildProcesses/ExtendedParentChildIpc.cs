// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedParentChildIpc.cs" company="">
//   
// </copyright>
// <summary>
//   TODO: Update summary.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;

    /// <summary>
    ///     TODO: Update summary.
    /// </summary>
    public class ExtendedParentChildIpc : ParentChildIpc, IExtendedParentChildIpc
    {
        #region Public Methods and Operators

        /// <summary>
        /// The send custom message to child.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void SendCustomMessageToChild(string message)
        {
            Console.WriteLine("Custom Message from Parent at " + DateTime.Now + ":" + message);
        }

        #endregion
    }
}