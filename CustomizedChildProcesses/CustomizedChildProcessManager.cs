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
        public override Type GetIChildParentIpcType()
        {
            return typeof(IChildParentIpc);
        }
    }
}
