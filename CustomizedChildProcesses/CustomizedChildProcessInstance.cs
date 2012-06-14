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
        public override Type GetIChildParentIpcType()
        {
            return typeof(IExtendedChildParentIpc);
        }

        public override Type GetParentChildIpcType()
        {
            return typeof(ExtendedParentChildIpc);
        }
    }
}
