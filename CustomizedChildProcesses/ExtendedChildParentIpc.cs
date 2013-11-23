// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExtendedChildParentIpc.cs" company="">
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
    public class ExtendedChildParentIpc : ChildParentIpc, IExtendedChildParentIpc
    {
        #region Public Methods and Operators

        /// <summary>
        /// The send custom message to parent.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void SendCustomMessageToParent(string message)
        {
            Console.WriteLine("Custm Message From Child: " + message);
        }

        #endregion
    }
}