// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomizedChildProcessManager.cs" company="">
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
    public class CustomizedChildProcessManager : ChildProcessManager
    {
        #region Methods

        /// <summary>
        ///     The get child parent ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected override Type GetChildParentIpcType()
        {
            return typeof(ExtendedChildParentIpc);
        }

        /// <summary>
        ///     The get i child parent ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected override Type GetIChildParentIpcType()
        {
            return typeof(IExtendedChildParentIpc);
        }

        /// <summary>
        ///     The get i parent child ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected override Type GetIParentChildIpcType()
        {
            return typeof(IExtendedParentChildIpc);
        }

        #endregion
    }
}