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
    using System.ServiceModel;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ServiceContract(CallbackContract = typeof(IExtendedParentChildIpc))]
    public interface IExtendedChildParentIpc : IChildParentIpc
    {
    }
}
