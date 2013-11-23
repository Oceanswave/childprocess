﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ChildProcessManager.cs" company="Maierhofer Software, Germany">
//   Copyright 2012 by Maierhofer Software, Germany
// </copyright>
// <summary>
//   The child process manager.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace ChildProcesses
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using System.ServiceModel;

    /// <summary>
    ///     The child process manager.
    /// </summary>
    public class ChildProcessManager : ProcessInstance, IEnumerable, IDisposable
    {
        #region Static Fields

        /// <summary>
        ///     The parent process singleton.
        /// </summary>
        private static ChildProcessManager parentProcessSinglelton;

        #endregion

        #region Fields

        /// <summary>
        ///     The child processes.
        /// </summary>
        private Dictionary<int, ChildProcess> childProcesses;

        /// <summary>
        ///     The child processes lock.
        /// </summary>
        private object childProcessesLock;

        /// <summary>
        ///     The IPC host.
        /// </summary>
        private ServiceHost ipcHost;

        /// <summary>
        ///     The IPC host lock.
        /// </summary>
        private object ipcHostLock = new object();

        #endregion

        #region Constructors and Destructors

        /// <summary>
        ///     Initializes a new instance of the <see cref="ChildProcessManager" /> class.
        /// </summary>
        /// <exception cref="InvalidOperationException"></exception>
        public ChildProcessManager()
        {
            if (parentProcessSinglelton != null)
            {
                throw new InvalidOperationException("ChildProcessManager Singleton already instantiated");
            }

            parentProcessSinglelton = this;
            this.childProcesses = new Dictionary<int, ChildProcess>();
            this.childProcessesLock = new object();
            this.ResetClientIpcHost();
        }

        /// <summary>
        /// Finalizes an instance of the <see cref="ChildProcessManager"/> class. 
        /// </summary>
        ~ChildProcessManager()
        {
            this.Dispose(false);
        }

        #endregion

        #region Delegates

        /// <summary>
        ///     The process state changed event handler.
        /// </summary>
        /// <param name="sender">
        ///     The sender.
        /// </param>
        /// <param name="e">
        ///     The e.
        /// </param>
        public delegate void ProcessStateChangedEventHandler(object sender, ProcessStateChangedEventArgs e);

        #endregion

        #region Public Events

        /// <summary>
        /// The child process started.
        /// </summary>
        public static event EventHandler<ProcessStartEventArgs> ChildProcessStarted;

        /// <summary>
        ///     The process state changed.
        /// </summary>
        public event ProcessStateChangedEventHandler ProcessStateChanged;

        #endregion

        #region Public Properties

        /// <summary>
        /// Gets or sets a value indicating whether child debugger auto attach.
        /// </summary>
        public static bool ChildDebuggerAutoAttach { get; set; }

        /// <summary>
        ///     Gets Current.
        /// </summary>
        public static ChildProcessManager Current
        {
            get
            {
                return parentProcessSinglelton;
            }
        }

        /// <summary>
        /// Gets a value indicating whether is disposed.
        /// </summary>
        public bool IsDisposed { get; private set; }

        #endregion

        #region Public Methods and Operators

        /// <summary>
        /// The dispose.
        /// </summary>
        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// The get enumerator.
        /// </summary>
        /// <returns>
        /// The <see cref="IEnumerator"/>.
        /// </returns>
        public IEnumerator GetEnumerator()
        {
            lock (this.childProcessesLock)
            {
                return this.childProcesses.Values.ToArray().GetEnumerator();
            }
        }

        /// <summary>
        ///     Watchdog Processing must periodically called to actualize state.
        /// </summary>
        public void ProcessWatchdog()
        {
            var exitedProcesses = new List<ChildProcess>();
            ChildProcess[] currentChildProcesses;
            lock (this.childProcessesLock)
            {
                currentChildProcesses = this.childProcesses.Values.ToArray();
            }

            bool sendAliveMessages = this.LastAliveMessage + this.AliveMessageFrquency < DateTime.Now;
            if (sendAliveMessages)
            {
                this.LastAliveMessage = DateTime.Now;
            }

            foreach (var childProcess in currentChildProcesses)
            {
                if (childProcess.Process.HasExited)
                {
                    exitedProcesses.Add(childProcess);
                }
                else
                {
                    if (!childProcess.ipcChannelAvail && childProcess.ParentChildIpc != null)
                    {
                        try
                        {
                            childProcess.ParentChildIpc.ParentIpcInit();
                            childProcess.ipcChannelAvail = true;
                            this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.IpcChannelAvail, null);
                        }
                        catch (Exception)
                        {
                            childProcess.ParentChildIpc = null;
                        }
                    }

                    if (sendAliveMessages)
                    {
                        try
                        {
                            if (childProcess.ParentChildIpc != null)
                            {
                                childProcess.ParentChildIpc.ParentAlive();
                            }
                        }
                        catch (Exception)
                        {
                            childProcess.ParentChildIpc = null;
                        }
                    }

                    if (!childProcess.ipcChannelAvail && childProcess.ParentChildIpc != null)
                    {
                        childProcess.ipcChannelAvail = true;
                        this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.IpcChannelAvail, null);
                    }

                    if (!childProcess.watchdogTimeout && childProcess.lastTimeAlive + this.WatchdogTimeout < DateTime.Now)
                    {
                        childProcess.watchdogTimeout = true;
                        this.RaiseProcessStateChangedEvent(childProcess, ProcessStateChangedAction.WatchdogTimeout, null);
                    }
                }
            }

            foreach (var exitedProcess in exitedProcesses)
            {
                this.RaiseProcessStateChangedEvent(exitedProcess, ProcessStateChangedAction.ChildExited, null);
            }

            lock (this.childProcessesLock)
            {
                foreach (var exitedProcess in exitedProcesses)
                {
                    this.childProcesses.Remove(exitedProcess.Process.Id);
                }
            }
        }

        /// <summary>
        /// The start child process.
        /// </summary>
        /// <param name="processInfo">
        /// The process info.
        /// </param>
        /// <returns>
        /// The <see cref="ChildProcess"/>.
        /// </returns>
        public ChildProcess StartChildProcess(ProcessStartInfo processInfo)
        {
            var childProcess = new ChildProcess();
            this.StartChildProcess(processInfo, childProcess);
            return childProcess;
        }

        /// <summary>
        /// The start child process.
        /// </summary>
        /// <param name="processInfo">
        /// The process info.
        /// </param>
        /// <param name="childProcess">
        /// The child process.
        /// </param>
        public void StartChildProcess(ProcessStartInfo processInfo, ChildProcess childProcess)
        {
            processInfo.CreateNoWindow = false;
            processInfo.RedirectStandardOutput = true;
            processInfo.RedirectStandardInput = true;
            processInfo.RedirectStandardError = true;
            processInfo.UseShellExecute = false;
            processInfo.EnvironmentVariables.Add("ChildProcesses.IpcChannelPrefix", "lkasdjf -asd");
            if (ChildDebuggerAutoAttach && Debugger.IsAttached)
            {
                processInfo.EnvironmentVariables.Add("ChildProcesses.AutoAttachDebugger", "True");
            }
            else
            {
                processInfo.EnvironmentVariables.Add("ChildProcesses.AutoAttachDebugger", "False");
            }

            var process = new Process();
            process.StartInfo = processInfo;
            childProcess.PreStartInit(this, process);
            process.Start();
            try
            {
                childProcess.PostStartInit();
                lock (this.childProcessesLock)
                {
                    this.childProcesses.Add(process.Id, childProcess);
                }

                OnChildProcessStarted(new ProcessStartEventArgs() { Manager = this, ProcessStartInfo = processInfo, StartedProcess = childProcess });
            }
            catch (Exception)
            {
                process.Kill();
                throw;
            }
        }

        /// <summary>
        /// The try get child process.
        /// </summary>
        /// <param name="id">
        /// The id.
        /// </param>
        /// <returns>
        /// The <see cref="ChildProcess"/>.
        /// </returns>
        public ChildProcess TryGetChildProcess(int id)
        {
            lock (this.childProcessesLock)
            {
                ChildProcess childProcess = null;
                this.childProcesses.TryGetValue(id, out childProcess);
                return childProcess;
            }
        }

        #endregion

        #region Methods

        /// <summary>
        /// The on child alive.
        /// </summary>
        /// <param name="childProcessId">
        /// The child process id.
        /// </param>
        protected internal virtual void OnChildAlive(int childProcessId)
        {
            ChildProcess child = this.TryGetChildProcess(childProcessId);
            if (child != null)
            {
                if (child.ParentChildIpc == null)
                {
                    OperationContext operationContext = OperationContext.Current;
                    Type operationContextType = operationContext.GetType();
                    MethodInfo getCallbackChannelMethod = operationContextType.GetMethod("GetCallbackChannel");

                    getCallbackChannelMethod = getCallbackChannelMethod.MakeGenericMethod(this.GetIParentChildIpcType());
                    var callback = (IParentChildIpc)getCallbackChannelMethod.Invoke(operationContext, null);
                    child.ParentChildIpc = callback;
                }

                child.watchdogTimeout = false;
                child.lastTimeAlive = DateTime.Now;
            }
        }

        /// <summary>
        /// The on child ipc init.
        /// </summary>
        /// <param name="childProcessId">
        /// The child process id.
        /// </param>
        protected internal virtual void OnChildIpcInit(int childProcessId)
        {
            this.OnChildAlive(childProcessId);
        }

        /// <summary>
        /// The raise process state changed event.
        /// </summary>
        /// <param name="childProcess">
        /// The child process.
        /// </param>
        /// <param name="action">
        /// The action.
        /// </param>
        /// <param name="data">
        /// The data.
        /// </param>
        protected internal virtual void RaiseProcessStateChangedEvent(ChildProcess childProcess, ProcessStateChangedAction action, string data)
        {
            // Raise the event by using the () operator.
            if (this.ProcessStateChanged != null)
            {
                this.ProcessStateChanged(this, new ProcessStateChangedEventArgs(childProcess, action, data));
            }
        }

        /// <summary>
        /// The dispose.
        /// </summary>
        /// <param name="disposing">
        /// The disposing.
        /// </param>
        protected virtual void Dispose(bool disposing)
        {
            this.IsDisposed = true;
            lock (this.childProcessesLock)
            {
                foreach (var childProcess in this.childProcesses)
                {
                    if (! childProcess.Value.Process.HasExited)
                    {
                        childProcess.Value.Process.Kill();
                    }
                }
            }
        }

        /// <summary>
        /// Gets the type of the child parent IPC channel.
        /// </summary>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        protected virtual Type GetChildParentIpcType()
        {
            return typeof(ChildParentIpc);
        }

        /// <summary>
        /// Gets the interface type of the child to parent IPC channel.
        /// </summary>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        protected virtual Type GetIChildParentIpcType()
        {
            return typeof(IChildParentIpc);
        }

        /// <summary>
        /// Gets the interface type of the parent to child IPC channel
        /// </summary>
        /// <returns>
        /// The <see cref="Type"/>.
        /// </returns>
        protected virtual Type GetIParentChildIpcType()
        {
            return typeof(IParentChildIpc);
        }

        /// <summary>
        /// The on child process started.
        /// </summary>
        /// <param name="e">
        /// The e.
        /// </param>
        private static void OnChildProcessStarted(ProcessStartEventArgs e)
        {
            EventHandler<ProcessStartEventArgs> handler = ChildProcessStarted;
            if (handler != null)
            {
                handler(null, e);
            }
        }

        /// <summary>
        /// The ipc host_ faulted.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void IpcHost_Faulted(object sender, EventArgs e)
        {
            lock (this.ipcHostLock)
            {
                if (this.ipcHost != null)
                {
                    this.ResetClientIpcHost();
                }
            }
        }

        /// <summary>
        ///     The reset client ipc host.
        /// </summary>
        private void ResetClientIpcHost()
        {
            lock (this.ipcHostLock)
            {
                if (this.ipcHost != null)
                {
                    this.ipcHost.Close();
                    this.ipcHost = null;
                }

                var ipcEndpint = (ChildParentIpc)Activator.CreateInstance(this.GetChildParentIpcType());
                ipcEndpint.Manager = this;
                var newHost = new ServiceHost(ipcEndpint, new[] { new Uri("net.pipe://localhost/" + this.GetIpcUrlPrefix() + "/" + this.CurrentProcess.Id) });
                newHost.AddServiceEndpoint(this.GetIChildParentIpcType(), new NetNamedPipeBinding(), "ParentChildIpc");
                newHost.Faulted += new EventHandler(this.IpcHost_Faulted);
                newHost.Open();
                this.ipcHost = newHost;
            }
        }

        #endregion

        /// <summary>
        /// The process start event args.
        /// </summary>
        public class ProcessStartEventArgs : EventArgs
        {
            #region Public Properties

            /// <summary>
            /// Gets or sets the manager.
            /// </summary>
            public ChildProcessManager Manager { get; set; }

            /// <summary>
            /// Gets the process start info.
            /// </summary>
            public ProcessStartInfo ProcessStartInfo { get; internal set; }

            /// <summary>
            /// Gets the started process.
            /// </summary>
            public ChildProcess StartedProcess { get; internal set; }

            #endregion
        }
    }
}