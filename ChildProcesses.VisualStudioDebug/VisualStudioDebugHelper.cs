// --------------------------------------------------------------------------------------------------------------------
// <copyright file="VisualStudioDebugHelper.cs" company="">
//   
// </copyright>
// <summary>
//   The visual studio debug helper.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses.VisualStudioDebug
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Runtime.InteropServices.ComTypes;

    using EnvDTE;

    using Process = System.Diagnostics.Process;

    /// <summary>
    /// The visual studio debug helper.
    /// </summary>
    public class VisualStudioDebugHelper
    {
        #region Constants


        /// <summary>
        ///  COM application is busy and can't handle the call
        /// </summary>
        private const int RPC_E_SERVERCALL_RETRYLATER = unchecked((int)0x8001010A);

        #endregion

        #region Static Fields

        /// <summary>
        /// The debugging DTE.
        /// </summary>
        private static DTE debuggingDte;

        #endregion

        #region Public Methods and Operators


        /// <summary>
        /// Attaches the debugger to the process.
        /// </summary>
        /// <param name="childProcessId">The child process unique identifier.</param>
        [STAThread]
        public static void AttachDebuggerToProcess(int childProcessId)
        {
            if (debuggingDte == null)
            {
                return;
            }

            EnvDTE.Process childProcess = null;
            try
            {
                IEnumerable<EnvDTE.Process> localProcesses = debuggingDte.Debugger.LocalProcesses.OfType<EnvDTE.Process>();

                foreach (var process in localProcesses)
                {
                    if (process.ProcessID == childProcessId)
                    {
                        childProcess = process;
                    }
                }

                if (childProcess != null)
                {
                    childProcess.Attach();
                }
            }
            catch (COMException e)
            {
                if (e.ErrorCode == RPC_E_SERVERCALL_RETRYLATER)
                {
                    return;
                }

                throw;
            }
        }


        /// <summary>
        /// Registers the Helper.
        /// </summary>
        public static void Register()
        {
            MessageFilter.Register();
            debuggingDte = GetDebuggingDteInstance();
            if (debuggingDte != null)
            {
                ChildProcessManager.ChildDebuggerAutoAttach = true;
                ChildProcessManager.ChildProcessStarted += ChildProcessManagerOnChildProcessStarted;
            }
        }


        /// <summary>
        /// Revokes the Helper.
        /// </summary>
        public static void Revoke()
        {
            MessageFilter.Revoke();
            ChildProcessManager.ChildDebuggerAutoAttach = false;
            ChildProcessManager.ChildProcessStarted -= ChildProcessManagerOnChildProcessStarted;
        }

        #endregion

        #region Methods

        /// <summary>
        /// The child process manager on child process started.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="processStartEventArgs">
        /// The process start event args.
        /// </param>
        private static void ChildProcessManagerOnChildProcessStarted(object sender, ChildProcessManager.ProcessStartEventArgs processStartEventArgs)
        {
            AttachDebuggerToProcess(processStartEventArgs.StartedProcess.Process.Id);
        }

        /// <summary>
        /// The create bind ctx.
        /// </summary>
        /// <param name="reserved">
        /// The reserved.
        /// </param>
        /// <param name="ppbc">
        /// The ppbc.
        /// </param>
        [DllImport("ole32.dll")]
        private static extern void CreateBindCtx(int reserved, out IBindCtx ppbc);

        /// <summary>
        /// The get debugging dte instance.
        /// </summary>
        /// <returns>
        /// The <see cref="DTE"/>.
        /// </returns>
        [STAThread]
        private static DTE GetDebuggingDteInstance()
        {
            int currentProcessId = Process.GetCurrentProcess().Id;

            foreach (var dte in GetVisualStudioInstances())
            {
                try
                {
                    if (dte.Debugger != null && dte.Debugger.DebuggedProcesses != null)
                    {
                        IEnumerable<EnvDTE.Process> debuggedProcesses = dte.Debugger.DebuggedProcesses.OfType<EnvDTE.Process>();

                        foreach (var process in debuggedProcesses)
                        {
                            if (process.ProcessID == currentProcessId)
                            {
                                return dte;
                            }
                        }
                    }
                }
                catch (COMException e)
                {
                    if (e.ErrorCode == RPC_E_SERVERCALL_RETRYLATER)
                    {
                        continue;
                    }

                    throw;
                }
            }

            return null;
        }

        /// <summary>
        /// The get running object table.
        /// </summary>
        /// <param name="reserved">
        /// The reserved.
        /// </param>
        /// <param name="prot">
        /// The prot.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("ole32.dll")]
        private static extern int GetRunningObjectTable(int reserved, out IRunningObjectTable prot);

        /// <summary>
        /// The get visual studio instances.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerable"/>.
        /// </returns>
        private static IEnumerable<DTE> GetVisualStudioInstances()
        {
            var result = new List<DTE>();

            IRunningObjectTable rot;
            IEnumMoniker enumMoniker;
            try
            {
                int retVal = GetRunningObjectTable(0, out rot);

                if (retVal == 0)
                {
                    rot.EnumRunning(out enumMoniker);

                    IntPtr fetched = IntPtr.Zero;
                    var moniker = new IMoniker[1];
                    while (enumMoniker.Next(1, moniker, fetched) == 0)
                    {
                        try
                        {
                            IBindCtx bindCtx;
                            CreateBindCtx(0, out bindCtx);
                            string displayName;
                            moniker[0].GetDisplayName(bindCtx, null, out displayName);
                            if (displayName.StartsWith("!VisualStudio"))
                            {
                                object rotObject;
                                rot.GetObject(moniker[0], out rotObject);
                                var dte = rotObject as DTE;
                                if (dte != null)
                                {
                                    result.Add(dte);
                                }
                            }
                        }
                        catch (COMException e)
                        {
                            if (e.ErrorCode == RPC_E_SERVERCALL_RETRYLATER)
                            {
                                continue;
                            }

                            throw;
                        }
                    }
                }
            }
            catch (COMException e)
            {
                if (e.ErrorCode == RPC_E_SERVERCALL_RETRYLATER)
                {
                    return result;
                }

                throw;
            }

            return result;
        }

        #endregion
    }

    /// <summary>
    /// The message filter.
    /// </summary>
    public class MessageFilter : IOleMessageFilter
    {
        // Class containing the IOleMessageFilter
        // thread error-handling functions.

        // Start the filter.

        // IOleMessageFilter functions.
        // Handle incoming thread requests.
        #region Public Methods and Operators

        /// <summary>
        /// The register.
        /// </summary>
        public static void Register()
        {
            IOleMessageFilter newFilter = new MessageFilter();
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(newFilter, out oldFilter);
        }

        // Done with the filter, close it.
        /// <summary>
        /// The revoke.
        /// </summary>
        public static void Revoke()
        {
            IOleMessageFilter oldFilter = null;
            CoRegisterMessageFilter(null, out oldFilter);
        }

        #endregion

        #region Explicit Interface Methods

        /// <summary>
        /// The handle in coming call.
        /// </summary>
        /// <param name="dwCallType">
        /// The dw call type.
        /// </param>
        /// <param name="hTaskCaller">
        /// The h task caller.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="lpInterfaceInfo">
        /// The lp interface info.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int IOleMessageFilter.HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo)
        {
            // Return the flag SERVERCALL_ISHANDLED.
            return 0;
        }

        /// <summary>
        /// The message pending.
        /// </summary>
        /// <param name="hTaskCallee">
        /// The h task callee.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="dwPendingType">
        /// The dw pending type.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int IOleMessageFilter.MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType)
        {
            // Return the flag PENDINGMSG_WAITDEFPROCESS.
            return 2;
        }

        /// <summary>
        /// The retry rejected call.
        /// </summary>
        /// <param name="hTaskCallee">
        /// The h task callee.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="dwRejectType">
        /// The dw reject type.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        int IOleMessageFilter.RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType)
        {
            if (dwRejectType == 2)
            {
                // flag = SERVERCALL_RETRYLATER.
                // Retry the thread call immediately if return >=0 & 
                // <100.
                return 99;
            }

            // Too busy; cancel call.
            return -1;
        }

        #endregion

        // Implement the IOleMessageFilter interface.
        #region Methods

        /// <summary>
        /// The co register message filter.
        /// </summary>
        /// <param name="newFilter">
        /// The new filter.
        /// </param>
        /// <param name="oldFilter">
        /// The old filter.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [DllImport("Ole32.dll")]
        private static extern int CoRegisterMessageFilter(IOleMessageFilter newFilter, out IOleMessageFilter oldFilter);

        #endregion
    }

    /// <summary>
    /// The OleMessageFilter interface.
    /// </summary>
    [ComImport]
    [Guid("00000016-0000-0000-C000-000000000046")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    internal interface IOleMessageFilter
    {
        /// <summary>
        /// The handle in coming call.
        /// </summary>
        /// <param name="dwCallType">
        /// The dw call type.
        /// </param>
        /// <param name="hTaskCaller">
        /// The h task caller.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="lpInterfaceInfo">
        /// The lp interface info.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [PreserveSig]
        int HandleInComingCall(int dwCallType, IntPtr hTaskCaller, int dwTickCount, IntPtr lpInterfaceInfo);

        /// <summary>
        /// The retry rejected call.
        /// </summary>
        /// <param name="hTaskCallee">
        /// The h task callee.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="dwRejectType">
        /// The dw reject type.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [PreserveSig]
        int RetryRejectedCall(IntPtr hTaskCallee, int dwTickCount, int dwRejectType);

        /// <summary>
        /// The message pending.
        /// </summary>
        /// <param name="hTaskCallee">
        /// The h task callee.
        /// </param>
        /// <param name="dwTickCount">
        /// The dw tick count.
        /// </param>
        /// <param name="dwPendingType">
        /// The dw pending type.
        /// </param>
        /// <returns>
        /// The <see cref="int"/>.
        /// </returns>
        [PreserveSig]
        int MessagePending(IntPtr hTaskCallee, int dwTickCount, int dwPendingType);
    }
}