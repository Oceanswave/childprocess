// -----------------------------------------------------------------------
// <copyright file="IExtendedChildParentIpc.cs" company="">
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
    public class ExtendedChildParentIpc : ChildParentIpc, IExtendedChildParentIpc 
    {
        public void SendCustomMessageToParent(string message)
        {
            Console.WriteLine("Custm Message From Child: " + message);
        }
    }
}
