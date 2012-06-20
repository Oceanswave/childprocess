// -----------------------------------------------------------------------
// <copyright file="CustomizedChildProcessManager.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class CustomizedChildProcessManager : ChildProcessManager
    {
        protected override Type GetIChildParentIpcType()
        {
            return typeof(IExtendedChildParentIpc);
        }

        protected override Type GetChildParentIpcType()
        {
            return typeof(ExtendedChildParentIpc);
        }

        protected override Type GetIParentChildIpcType()
        {
            return typeof(IExtendedParentChildIpc);
        }
    }
}
