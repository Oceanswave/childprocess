// -----------------------------------------------------------------------
// <copyright file="CustomizedChildProcess.cs" company="">
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
    public class CustomizedChildProcessInstance : ChildProcessInstance
    {
        protected override Type GetIChildParentIpcType()
        {
            return typeof(IExtendedChildParentIpc);
        }

        protected override Type GetParentChildIpcType()
        {
            return typeof(ExtendedParentChildIpc);
        }
    }
}
