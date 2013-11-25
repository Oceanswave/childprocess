// --------------------------------------------------------------------------------------------------------------------
// <copyright file="CustomizedChildProcessInstance.cs" company="">
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
    public class CustomizedChildProcessInstance : ChildProcessInstance
    {
        #region Methods

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
        ///     The get parent child ipc type.
        /// </summary>
        /// <returns>
        ///     The <see cref="Type" />.
        /// </returns>
        protected override Type GetParentChildIpcType()
        {
            return typeof(ExtendedParentChildIpc);
        }

        #endregion
    }
}