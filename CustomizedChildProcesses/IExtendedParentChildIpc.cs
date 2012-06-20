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
    using System.ServiceModel;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    [ServiceContract]
    public interface IExtendedParentChildIpc : IParentChildIpc
    {
        [OperationContract]
        void SendCustomMessageToChild(string message);
    }
}
