// -----------------------------------------------------------------------
// <copyright file="IExtendedParentChildIpc.cs" company="">
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
    public class ExtendedParentChildIpc: ParentChildIpc, IExtendedParentChildIpc
    {
        public void SendCustomMessageToClient(string message)
        {
            Console.WriteLine("Custom Message from Parent at " + DateTime.Now + ":"  + message );
        }
    }
}
